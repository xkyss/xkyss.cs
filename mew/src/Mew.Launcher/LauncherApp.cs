using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Icon = System.Drawing.Icon;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mew.Workbench;
using WorkbenchType = Mew.Workbench.Workbench;

namespace Mew.Launcher;

/// <summary>
/// Launcher 应用本体:编译期组装 Workbench 五区,持有启动项数据与全部交互状态。
/// </summary>
internal sealed class LauncherApp
{
    private const string AppVersion = "v0.1.5";
    private const string DefaultOverlayHotkey = "Ctrl+Alt+Space";
    private const string RevealDocumentHotkey = "Ctrl+Alt+R";
    private const string SettingsDocumentId = "settings-document";
    private const string DetailDocumentId = "detail";
    private const string ItemsDocumentId = "items";
    private static readonly Color HotkeyWarning = Color.FromArgb(255, 200, 60, 60);

    private readonly LauncherStore _store = new();
    private readonly SettingsStore _settings = new();
    private readonly ObservableValue<string> _hotkeyStatus;
    private string _overlayHotkey;
    private Window? _window;
    private Icon? _windowIcon;
    private bool _capturingHotkey;
    private Button? _hotkeyChangeButton;
    private Button? _titleThemeButton;
    private Label? _hotkeyDisplay;
    private Label? _hotkeyHint;
    private readonly LauncherRunner _runner = new();
    private readonly LaunchDebouncer _launchDebouncer = new(TimeSpan.FromMilliseconds(500));
    private readonly IconResolver _icons = new();
    private readonly ObservableValue<string> _launchStatus = new("就绪");
    private readonly List<LauncherItem> _items;
    private readonly ItemHotkeys _itemHotkeys;
    private readonly WorkbenchType _workbench = new();
    private readonly WorkbenchThemeContext _theme;
    private readonly StackPanel _listPanel = new();
    private readonly SelectionModel _listSelection = new();
    private readonly List<Button> _listButtons = [];
    private readonly List<Border> _listBars = [];
    private List<LauncherItem> _listItems = [];
    private int _styledSelection = -1;
    private int _hoveredIndex = -1;
    private ScrollViewer? _listScrollViewer;
    private readonly StackPanel _detailPanel = new();
    private readonly StackPanel _logPanel = new();
    private string _navId = LauncherData.AllNavId; // 当前导航节点:「全部」/ 分类 id /「未分类」
    private LauncherItem? _current;
    private bool _loading;
    private TreeView? _tree;
    private TextBox? _itemsSearchBox;
    private TextBox? _categorySearchBox;
    private StackPanel? _categoryTreePanel;
    private string _viewMode = "card"; // 启动项列表形态:card(卡片,默认)/ list(列表),持久化于 settings.json
    private string _query = "";
    private Button? _modeToggleButton;
    private readonly Dictionary<string, string> _sideBarFilters = [];
    private TreeItemsView<CategoryTreeNode>? _treeItems;
    private const string SettingsAppearance = "appearance";
    private const string SettingsHotkey = "hotkey";
    private const string SettingsData = "data";
    private string _settingsNav = SettingsAppearance; // 设置上下文当前分类
    private StackPanel? _settingsContent;

    internal LauncherApp()
    {
        _items = _store.Load();
        _theme = _workbench.ThemeContext;
        _itemHotkeys = new ItemHotkeys(_items, LaunchItem, Feedback);
        var settings = _settings.Load();
        _overlayHotkey = string.IsNullOrWhiteSpace(settings.OverlayHotkey) ? DefaultOverlayHotkey : settings.OverlayHotkey!;
        _hotkeyStatus = new ObservableValue<string>(_overlayHotkey);
        _viewMode = string.IsNullOrWhiteSpace(settings.ItemsViewMode) ? "card" : settings.ItemsViewMode!;
    }

    internal void Run()
    {
        var window = new NativeChromeWindow()
            .Title($"Mew Launcher — {AppVersion}")
            .Resizable(1080, 720);

        TrayIcon? tray = null;
        _window = window;
        window.PreviewKeyDown += OnWindowKeyDown;

        _titleThemeButton = BuildTitleBar(window, Quit, OpenSettings, CycleTheme, _workbench);
        UpdateThemeButton(); // 初始图标/提示跟随已加载的主题模式

        _workbench
            .Theme(theme => theme
                .SetMode(LoadThemeMode())
                .SetAccent(Accent.Blue))
            .ActivityBar(bar =>
            {
                bar.Item("launch", "启动", GlyphKind.Hamburger);
                bar.Item("settings", "设置", SettingsGlyph());
            })
            .SideBar(side => side
                .View("launch", "启动", BuildCategoryTree())
                .View("settings", "设置", BuildSettingsSideBar()))
            .EditorArea(editor => editor
                .Document(ItemsDocumentId, "启动项", BuildItemsDocument())
                .Document(DetailDocumentId, "启动项详情", _detailPanel)
                .Document(SettingsDocumentId, "设置", BuildSettingsDocument()))
            .Panel(panel => panel.View("output", "输出", BuildOutputPanel()))
            .StatusBar(status => status
                .Item("launch", _launchStatus)
                .Item("shortcut", _hotkeyStatus));

        ShowNav(_navId);
        ShowEmptyDetail();
        MigrateLegacySettingsDocumentLayout();

        window.Content = _workbench.Build();

        var overlay = new OverlayWindow(window, _items, _runner, _icons, _theme);

        window.Closing += e =>
        {
            e.Cancel = true;
            window.Hide();
        };

        window.Loaded += () =>
        {
            _workbench.RefreshPresentation();
            ApplyWindowIcon(window);

            if (!GlobalHotkey.Register(window.Handle, _overlayHotkey))
            {
                AppendLog($"⚠ 呼出热键 {_overlayHotkey} 注册失败(可能已被其他程序占用)");
            }

            _itemHotkeys.Attach(window.Handle);
            _itemHotkeys.RegisterAll();
            tray = new TrayIcon(window.Handle, ShowMain, Quit);
            tray.Add();

            // 主题模式变更(状态栏循环按钮 / 设置页 / 标题栏)统一持久化并同步各处显示
            if (Application.Current is { } app)
            {
                app.ThemeModeChanged += PersistThemeMode;
                app.ThemeModeChanged += SyncThemeRadios;
                app.ThemeModeChanged += UpdateThemeButton;
                app.ThemeModeChanged += ReapplyListSelection;
            }

        };

        window.NativeMessage += args =>
        {
            if (args is not Win32NativeMessageEventArgs e)
            {
                return;
            }

            if (e.Msg == GlobalHotkey.WmHotkey)
            {
                var id = (int)e.WParam;
                if (id == GlobalHotkey.OverlayHotkeyId)
                {
                    overlay.ShowOverlay();
                }
                else
                {
                    _itemHotkeys.TryLaunch(id);
                }
                args.Handled = true;
            }
            else if (e.Msg == TrayIcon.WmCallback && tray is not null)
            {
                tray.HandleCallback((uint)e.WParam, (uint)e.LParam);
                args.Handled = true;
            }
        };

        Application.Run(window);
        _windowIcon?.Dispose();

        void ShowMain()
        {
            window.Show(null!);
            window.Activate();
        }

        void Quit()
        {
            tray?.Dispose();
            Application.Quit();
        }
    }

    /// <summary>标题栏:左区图标 + 菜单栏(File=设置/退出、View=区域显隐、Help=关于)、右区「切换主题」图标按钮。</summary>
    private static Button BuildTitleBar(NativeChromeWindow window, Action quit, Action openSettings, Action cycleTheme, WorkbenchType workbench)
    {
        var appIcon = IconResolver.ExtractIcon(Environment.ProcessPath!);
        if (appIcon is not null)
        {
            window.TitleBarLeft.Add(new Image()
                .Source(appIcon)
                .Size(20, 20)
                .Margin(new Thickness(6, 0, 6, 0)));
        }

        var menuBar = new MenuBar()
            .Height(28)
            .DrawBottomSeparator(false)
            .Background(Color.Transparent)
            .Items(
                new MenuItem("_File").Menu(
                    new Menu()
                        .Item("设置", openSettings)
                        .Separator()
                        .Item("退出", quit)),
                new MenuItem("_View").Menu(BuildViewMenu(workbench)),
                new MenuItem("_Help").Menu(
                    new Menu().Item("关于", () => ShowAbout(window)))
            );

        window.TitleBarLeft.Add(menuBar);

        // 右区:切换主题图标按钮(图标与提示由 UpdateThemeButton 随模式刷新)
        var themeButton = new Button()
            .Content(new Label().Text(""))
            .ToolTip("切换主题")
            .OnClick(cycleTheme)
            .CanDrag(false)
            .Size(36, 28);
        window.TitleBarRight.Add(themeButton);
        return themeButton;
    }

    /// <summary>查看菜单:显示/隐藏 侧边栏、底部面板、活动栏、状态栏;菜单项文本随当前显隐状态反转。</summary>
    private static Menu BuildViewMenu(WorkbenchType workbench) => new Menu()
        .Add(ViewToggleItem(workbench, workbench.ToggleSideBar, () => workbench.IsSideBarVisible, "侧边栏"))
        .Add(ViewToggleItem(workbench, workbench.TogglePanel, () => workbench.IsPanelVisible, "底部面板"))
        .Add(ViewToggleItem(workbench, workbench.ToggleActivityBar, () => workbench.IsActivityBarVisible, "活动栏"))
        .Add(ViewToggleItem(workbench, workbench.ToggleStatusBar, () => workbench.IsStatusBarVisible, "状态栏"));

    /// <summary>构造「(显示/隐藏)xx」菜单项:文本反映当前状态,点击切换后文本反转。</summary>
    private static MenuItem ViewToggleItem(WorkbenchType workbench, Action toggle, Func<bool> isVisible, string label)
    {
        var item = new MenuItem("");
        item.Click = () =>
        {
            toggle();
            item.Text = isVisible() ? $"隐藏{label}" : $"显示{label}";
        };
        item.Text = isVisible() ? $"隐藏{label}" : $"显示{label}";
        return item;
    }

    /// <summary>循环切换主题模式(跟随系统 → 亮 → 暗),持久化与标题栏图标由 ThemeModeChanged 统一处理。</summary>
    private void CycleTheme()
    {
        var next = _theme.Mode switch
        {
            ThemeVariant.System => ThemeVariant.Light,
            ThemeVariant.Light => ThemeVariant.Dark,
            _ => ThemeVariant.System,
        };
        _theme.SetMode(next);
    }

    /// <summary>标题栏主题按钮:图标/提示跟随当前模式(☀ 浅色 / ☾ 暗色 / 🌓 跟随系统)。</summary>
    private void UpdateThemeButton()
    {
        if (_titleThemeButton is not { } button)
        {
            return;
        }

        var mode = _theme.Mode;
        button.Content(new Label().Text(ThemeIcon(mode)).FontSize(14));
        button.ToolTip(ThemeToolTip(mode));
    }

    private static string ThemeIcon(ThemeVariant mode) => mode switch
    {
        ThemeVariant.Light => "☀",
        ThemeVariant.Dark => "☾",
        _ => "🌓",
    };

    private static string ThemeToolTip(ThemeVariant mode) => mode switch
    {
        ThemeVariant.Light => "浅色 · 点击切换",
        ThemeVariant.Dark => "暗色 · 点击切换",
        _ => "跟随系统 · 点击切换",
    };

    private static void ShowAbout(NativeChromeWindow window)
    {
        MessageBox.Notify(
            $"Mew Launcher {AppVersion}\n基于 MewUI 与 Workbench 的应用启动管理器。",
            PromptIconKind.Info,
            "关于 Mew Launcher",
            window);
    }

    /// <summary>从 settings.json 读取主题模式,缺省/无效回退跟随系统。</summary>
    private ThemeVariant LoadThemeMode()
    {
        var stored = _settings.Load().ThemeMode;
        return Enum.TryParse<ThemeVariant>(stored, out var mode) ? mode : ThemeVariant.System;
    }

    /// <summary>将当前主题模式持久化到 settings.json(保留其他设置字段)。</summary>
    private void PersistThemeMode()
    {
        var settings = _settings.Load();
        settings.ThemeMode = _theme.Mode.ToString();
        _settings.Save(settings);
    }

    /// <summary>设置页单选跟随当前主题模式(状态栏按钮等外部切换时同步)。</summary>
    private void SyncThemeRadios()
    {
        if (_themeRadios is not { } radios)
        {
            return;
        }

        foreach (var (radio, mode) in radios.Zip(_themeModes))
        {
            radio.IsChecked = _theme.Mode == mode.Mode;
        }
    }

    /// <summary>设置活动栏图标:Label 撑满按钮内容区(Padding 已清零),文本居中。</summary>
    private UIElement SettingsGlyph() => new Label
    {
        Text = "⚙",
        FontSize = 18,
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
    }
    .WithTheme((_, label) => label.Foreground(_theme.ActivityBar.Foreground));

    /// <summary>标题栏齿轮与 File→设置:选择设置上下文并显式打开设置文档。</summary>
    private void OpenSettings()
    {
        _workbench.SelectActivity("settings");
        _workbench.OpenDocument(SettingsDocumentId);
    }

    /// <summary>
    /// 将旧布局中央区域里与设置侧边栏共用的 <c>settings</c> 组件迁移为独立的设置文档 ID。
    /// 仅修改中央布局树，侧边栏边框中的同名组件保持不变。
    /// </summary>
    private static void MigrateLegacySettingsDocumentLayout()
    {
        var layoutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mew", "layout.json");

        try
        {
            if (!File.Exists(layoutPath))
            {
                return;
            }

            var root = JsonNode.Parse(File.ReadAllText(layoutPath))?.AsObject();
            if (root is null || !MigrateLegacySettingsDocumentComponent(root["layout"]))
            {
                return;
            }

            File.WriteAllText(layoutPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (JsonException)
        {
        }
    }

    private static bool MigrateLegacySettingsDocumentComponent(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            var changed = obj["component"]?.GetValue<string>() == "settings";
            if (changed)
            {
                obj["component"] = SettingsDocumentId;
            }

            foreach (var child in obj)
            {
                changed |= MigrateLegacySettingsDocumentComponent(child.Value);
            }

            return changed;
        }

        if (node is JsonArray array)
        {
            return array.Any(MigrateLegacySettingsDocumentComponent);
        }

        return false;
    }

    private void ApplyWindowIcon(Window window)
    {
        _windowIcon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        if (_windowIcon is null)
        {
            return;
        }

        SendMessage(window.Handle, WmSetIcon, IconSmall, _windowIcon.Handle);
        SendMessage(window.Handle, WmSetIcon, IconBig, _windowIcon.Handle);
    }

    private const uint WmSetIcon = 0x0080;
    private static readonly IntPtr IconSmall = IntPtr.Zero;
    private static readonly IntPtr IconBig = new(1);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>窗口内快捷键:定位当前启动项详情所属的侧边栏分类;启动项列表激活时提供键盘导航。</summary>
    private void OnWindowKeyDown(KeyEventArgs e)
    {
        if (_capturingHotkey)
        {
            return;
        }

        if (e.ControlKey && e.AltKey && e.Key == Key.R)
        {
            if (_workbench.RevealDocument(DetailDocumentId))
            {
                Feedback($"已执行:在侧边栏定位 ({RevealDocumentHotkey})");
            }

            return;
        }

        // 列表键盘导航:仅当启动项列表文档激活且焦点不在文本输入内时生效(搜索/表单输入不劫持)
        if (_workbench.ActiveDocumentId == ItemsDocumentId && !IsTextInputFocused())
        {
            // 「/」聚焦搜索:MewUI Key 枚举无标点键,经平台虚拟键码 VK_OEM_2(0xBF)识别
            if (e.PlatformKey == 0xBF)
            {
                FocusItemsSearch();
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Up:
                    MoveListSelection(-1);
                    e.Handled = true;
                    break;
                case Key.Down:
                    MoveListSelection(+1);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    LaunchSelected();
                    e.Handled = true;
                    break;
                case Key.F when e.ControlKey:
                    FocusItemsSearch();
                    e.Handled = true;
                    break;
            }
        }
    }

    /// <summary>设置页主题单选(跟随系统/亮/暗),状态栏外部切换时保持同步。</summary>
    private List<RadioButton>? _themeRadios;
    private (ThemeVariant Mode, string Label)[] _themeModes =
    [
        (ThemeVariant.System, "跟随系统"),
        (ThemeVariant.Light, "亮色"),
        (ThemeVariant.Dark, "暗色"),
    ];

    /// <summary>编辑器区「设置」文档:内容容器,随侧边栏设置分类切换(外观/热键/数据)。</summary>
    private UIElement BuildSettingsDocument()
    {
        var content = new StackPanel();
        _settingsContent = content;
        ShowSettingsNav(_settingsNav);
        return content;
    }

    /// <summary>侧边栏「设置」上下文:三个设置分类(外观/热键/数据)。</summary>
    private UIElement BuildSettingsSideBar() => new StackPanel()
        .Padding(12)
        .Spacing(4)
        .Children(
            SettingsNavButton("外观", SettingsAppearance),
            SettingsNavButton("热键", SettingsHotkey),
            SettingsNavButton("数据", SettingsData)
        );

    private UIElement SettingsNavButton(string label, string id) => new Button()
        .Content(new Label().Text(label)
            .WithTheme((_, l) => l.Foreground(_theme.SideBar.Foreground)))
        .OnClick(() => ShowSettingsNav(id))
        .CanDrag(false)
        .WithTheme((_, button) => button.Background(_theme.SideBar.Background));

    /// <summary>切换设置分类并刷新编辑器区设置内容。</summary>
    private void ShowSettingsNav(string id)
    {
        _settingsNav = id;
        // 设置分类是侧边栏到编辑器区的导航，选择时确保对应文档可见。
        _workbench.OpenDocument(SettingsDocumentId);
        if (_settingsContent is not { } content)
        {
            return;
        }

        content.Clear();
        content.Add(id switch
        {
            SettingsHotkey => BuildHotkeyPanel(),
            SettingsData => BuildDataPanel(),
            _ => BuildAppearancePanel(),
        });
    }

    /// <summary>设置分类内容:外观(主题三选一,即时生效并持久化)。</summary>
    private UIElement BuildAppearancePanel()
    {
        var theme = _theme;
        var radios = _themeModes
            .Select(m => new RadioButton()
                .GroupName("theme")
                .IsChecked(theme.Mode == m.Mode)
                .Content(new Label().Text(m.Label)))
            .ToList();
        _themeRadios = radios;

        foreach (var (radio, mode) in radios.Zip(_themeModes))
        {
            radio.OnCheckedChanged(isChecked =>
            {
                if (isChecked)
                {
                    theme.SetMode(mode.Mode);
                }
            });
        }

        return new StackPanel()
            .Padding(24)
            .Spacing(12)
            .Children(
                new Label().Text("外观").FontSize(20).Bold()
                    .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                new Label().Text("主题").FontSize(14)
                    .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                new StackPanel()
                    .Spacing(6)
                    .Children(radios.Cast<Element>().ToArray())
            );
    }

    /// <summary>设置分类内容:热键(呼出热键捕获改绑、冲突与格式提示)。</summary>
    private UIElement BuildHotkeyPanel()
    {
        var theme = _theme;
        _hotkeyDisplay = new Label()
            .Text(_overlayHotkey)
            .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground));
        _hotkeyChangeButton = new Button()
            .Content(new Label().Text("更改"))
            .ToolTip("点击后按下新的组合键")
            .OnClick(StartCaptureHotkey)
            .CanDrag(false);
        _hotkeyHint = new Label()
            .Text("")
            .FontSize(11)
            .WithTheme((_, label) => label.Foreground(HotkeyWarning));

        return new StackPanel()
            .Padding(24)
            .Spacing(12)
            .Children(
                new Label().Text("热键").FontSize(20).Bold()
                    .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                new Label().Text("呼出热键").FontSize(14)
                    .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Children(_hotkeyDisplay, _hotkeyChangeButton),
                _hotkeyHint
            );
    }

    /// <summary>设置分类内容:数据(启动项数据文件路径与打开所在文件夹)。</summary>
    private UIElement BuildDataPanel()
    {
        var theme = _theme;
        var dataFilePath = _store.FilePath;

        return new StackPanel()
            .Padding(24)
            .Spacing(12)
            .Children(
                new Label().Text("数据").FontSize(20).Bold()
                    .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                new Label().Text("数据文件").FontSize(14)
                    .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Children(
                        new Label()
                            .Text(dataFilePath)
                            .FontSize(12)
                            .MaxWidth(460)
                            .TextWrapping(TextWrapping.Wrap)
                            .WithTheme((_, label) => label.Foreground(theme.EditorArea.Foreground)),
                        new Button()
                            .Content(new Label().Text("打开所在文件夹"))
                            .ToolTip("在资源管理器中定位 launcher.json")
                            .OnClick(() => OpenDataFolder(dataFilePath))
                            .CanDrag(false)
                    )
            );
    }

    /// <summary>在资源管理器中打开数据文件所在文件夹并选中该文件。</summary>
    private static void OpenDataFolder(string filePath)
    {
        try
        {
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
        catch (Exception ex) when (ex is Win32Exception or IOException or InvalidOperationException)
        {
        }
    }

    /// <summary>进入热键捕获模式:下一次按键组合作为新呼出热键(Esc 取消)。</summary>
    private void StartCaptureHotkey()
    {
        if (_window is null || _capturingHotkey)
        {
            return;
        }

        _capturingHotkey = true;
        _hotkeyChangeButton!.Content(new Label().Text("请按下新热键…"));
        _hotkeyHint!.Text = "按 Esc 取消";
        _window.PreviewKeyDown += OnCaptureKeyDown;
    }

    private void OnCaptureKeyDown(KeyEventArgs e)
    {
        if (!_capturingHotkey)
        {
            return;
        }

        e.Handled = true;

        if (e.Key == Key.Escape)
        {
            CancelCaptureHotkey();
            _hotkeyHint!.Text = "";
            return;
        }

        var parts = new List<string>();
        if (e.ControlKey)
        {
            parts.Add("Ctrl");
        }
        if (e.AltKey)
        {
            parts.Add("Alt");
        }
        if (e.ShiftKey)
        {
            parts.Add("Shift");
        }
        if (e.MetaKey)
        {
            parts.Add("Win");
        }

        var keyName = KeyToName(e.Key);
        if (keyName.Length == 0)
        {
            _hotkeyHint!.Text = "请按字母/数字/功能键组合(如 Ctrl+Shift+1)";
            return;
        }
        if (parts.Count == 0)
        {
            _hotkeyHint!.Text = "需要至少一个修饰键(Ctrl/Alt/Shift/Win)";
            return;
        }

        parts.Add(keyName);
        ApplyOverlayHotkey(string.Join("+", parts));
    }

    private void CancelCaptureHotkey()
    {
        if (!_capturingHotkey)
        {
            return;
        }

        _capturingHotkey = false;
        _window!.PreviewKeyDown -= OnCaptureKeyDown;
        _hotkeyChangeButton!.Content(new Label().Text("更改"));
    }

    /// <summary>校验、冲突检测、重新注册并持久化新呼出热键;失败时保持原热键并在设置页提示。</summary>
    private void ApplyOverlayHotkey(string hotkey)
    {
        if (!HotkeyParser.TryParse(hotkey, out var modifiers, out var vk))
        {
            _hotkeyHint!.Text = "不支持的热键组合";
            return;
        }

        // 与每项热键冲突检测
        var conflict = _items.FirstOrDefault(item =>
            !string.IsNullOrWhiteSpace(item.Hotkey)
            && HotkeyParser.TryParse(item.Hotkey, out var m, out var k)
            && m == modifiers && k == vk);
        if (conflict is not null)
        {
            _hotkeyHint!.Text = $"与启动项「{conflict.Name}」的每项热键冲突";
            return;
        }

        var handle = _window!.Handle;
        GlobalHotkey.Unregister(handle);
        if (!GlobalHotkey.Register(handle, hotkey))
        {
            GlobalHotkey.Register(handle, _overlayHotkey); // 恢复旧热键
            _hotkeyHint!.Text = "注册失败(可能已被其他程序占用)";
            return;
        }

        _overlayHotkey = hotkey;
        _hotkeyDisplay!.Text = hotkey;
        _hotkeyStatus.Value = hotkey;

        var settings = _settings.Load();
        settings.OverlayHotkey = hotkey;
        _settings.Save(settings);

        CancelCaptureHotkey();
        _hotkeyHint!.Text = $"已生效:{hotkey}";
    }

    private static string KeyToName(Key key) => key switch
    {
        Key.Space => "Space",
        Key.Enter => "Enter",
        Key.Escape => "Escape",
        Key.Tab => "Tab",
        Key.Backspace => "Backspace",
        Key.Insert => "Insert",
        Key.Delete => "Delete",
        Key.Home => "Home",
        Key.End => "End",
        Key.PageUp => "PageUp",
        Key.PageDown => "PageDown",
        Key.Left => "Left",
        Key.Right => "Right",
        Key.Up => "Up",
        Key.Down => "Down",
        >= Key.D0 and <= Key.D9 => ((char)(key - Key.D0 + '0')).ToString(),
        >= Key.A and <= Key.Z => ((char)(key - Key.A + 'A')).ToString(),
        >= Key.F1 and <= Key.F24 => "F" + (key - Key.F1 + 1),
        _ => "",
    };

    /// <summary>侧边栏「启动」上下文:分类搜索 + 分类导航树(「全部」置顶、分类树、「未分类」收尾)。</summary>
    private UIElement BuildCategoryTree()
    {
        var searchBox = new TextBox
        {
            Placeholder = "搜索分类",
            Text = SideBarFilter("launch"),
            CanDrag = false,
        };
        searchBox.TextChanged += text =>
        {
            _sideBarFilters["launch"] = text;
            RefreshCategoryTree();
        };
        _categorySearchBox = searchBox;

        var tree = new TreeView
        {
            SelectionMode = ItemsSelectionMode.Single,
            ExpandTrigger = TreeViewExpandTrigger.ClickChevron,
            CanDrag = false,
        };
        _tree = tree;
        tree.SelectionChanged += OnNavSelectionChanged;
        tree.MouseUp += OnCategoryTreeRightClick;

        var content = new StackPanel().Spacing(6);
        _categoryTreePanel = content;
        RefreshCategoryTree(); // 构建树项并默认选中「全部」;空态时面板内为引导视图

        return new StackPanel()
            .Padding(12)
            .Spacing(6)
            .Children(
                searchBox,
                content
            );
    }

    /// <summary>(重新)构建分类树项:按分类搜索词过滤,并默认选中「全部」;过滤为空时显示「无匹配分类」引导。</summary>
    private void RefreshCategoryTree()
    {
        var filtered = LauncherData.FilterNavTree(
            LauncherData.BuildNavTree(_store.Categories.ToList()), SideBarFilter("launch"));
        _treeItems = new TreeItemsView<CategoryTreeNode>(
            filtered,
            node => node.Children,
            node => node.Name,
            node => node.Id,
            node => node.Children.Count > 0);
        _tree!.ItemsSource = _treeItems;

        _categoryTreePanel!.Clear();
        if (filtered.Count == 0)
        {
            _categoryTreePanel.Add(EmptyStateView("无匹配分类", "清空搜索", ClearCategorySearch,
                _theme.SideBar.Foreground, _theme.SideBar.Background));
            return;
        }

        _categoryTreePanel.Add(_tree);
        _treeItems.SelectSingle(0);
    }

    private string SideBarFilter(string activityId) => _sideBarFilters.GetValueOrDefault(activityId, "");

    /// <summary>分类树右键:固定节点(「全部」「未分类」)无操作,用户分类弹出 新建子分类/重命名/删除。</summary>
    private void OnCategoryTreeRightClick(MouseEventArgs e)
    {
        if (!e.RightButton || _tree is not { } tree || _treeItems is not { } items)
        {
            return;
        }

        if (!tree.TryGetItemIndexAt(e, out var index))
        {
            return;
        }

        if (items.GetItem(index) is not CategoryTreeNode node || node.IsFixed)
        {
            return;
        }

        new ContextMenu(new Menu()
            .Item("新建子分类", () => AddSubCategory(node.Id))
            .Separator()
            .Item("重命名", () => RenameCategory(node.Id))
            .Item("删除", () => DeleteCategory(node.Id)))
            .ShowAt(tree, e.ScreenPosition);
    }

    /// <summary>删除分类:确认(提示启动项数量)→ 连根删(子分类与项全删)→ 刷新树与列表。</summary>
    private void DeleteCategory(string categoryId)
    {
        var categories = _store.Categories.ToList();
        var removed = LauncherData.AggregateSubtree(categories, _items, categoryId);
        var name = LauncherData.CategoryName(categories, categoryId) ?? categoryId;

        var confirmed = MessageBox.Confirm(
            $"删除分类「{name}」将连同其子分类一起删除,共 {removed.Count} 个启动项,且不可恢复。",
            PromptIconKind.Warning,
            "",
            _window!);
        if (!confirmed)
        {
            return;
        }

        var removedIds = new HashSet<string>(removed.Select(i => i.Id), StringComparer.Ordinal);
        _items.RemoveAll(i => removedIds.Contains(i.Id));
        _store.UpdateCategories(LauncherData.RemoveCategoryNode(categories, categoryId));
        _store.Save(_items);
        _itemHotkeys.RegisterAll();

        // 当前导航在被删分类(或其子孙)内 → 回退「全部」
        if (_navId is not LauncherData.AllNavId and not LauncherData.UncategorizedNavId
            && LauncherData.Find(_store.Categories.ToList(), _navId) is null)
        {
            _navId = LauncherData.AllNavId;
        }

        RefreshCategoryTree();
        ShowNav(_navId);
        ShowEmptyDetail(); // 若详情正显示被删分类的启动项,清空
    }

    /// <summary>在指定分类下新建子分类(轻量输入对话框)。</summary>
    private void AddSubCategory(string parentId)
    {
        PromptText("新建子分类", "", name =>
        {
            if (name.Trim().Length == 0)
            {
                return;
            }

            var categories = _store.Categories.ToList();
            var newNode = new LauncherCategory("cat-" + Guid.NewGuid().ToString("N")[..4], name.Trim(), []);
            _store.UpdateCategories(LauncherData.AddCategoryNode(categories, parentId, newNode));
            _store.Save(_items);
            RefreshCategoryTree();
        });
    }

    /// <summary>重命名分类(轻量输入对话框)。</summary>
    private void RenameCategory(string categoryId)
    {
        var node = LauncherData.Find(_store.Categories.ToList(), categoryId);
        if (node is null)
        {
            return;
        }

        PromptText("重命名分类", node.Name, name =>
        {
            if (name.Trim().Length == 0)
            {
                return;
            }

            var categories = _store.Categories.ToList();
            _store.UpdateCategories(LauncherData.RenameCategoryNode(categories, categoryId, name.Trim()));
            _store.Save(_items);
            RefreshCategoryTree();
        });
    }

    /// <summary>轻量模态文本输入对话框(新建/重命名分类用)。</summary>
    private void PromptText(string title, string initial, Action<string> onConfirm)
    {
        var input = new TextBox
        {
            Text = initial,
            CanDrag = false,
        };

        var dialog = new Window()
            .Title(title)
            .Fixed(380, 170);
        var confirm = new Button()
            .Content(new Label().Text("确定"))
            .OnClick(() =>
            {
                dialog.Close();
                onConfirm(input.Text);
            })
            .CanDrag(false);
        var cancel = new Button()
            .Content(new Label().Text("取消"))
            .OnClick(() => dialog.Close())
            .CanDrag(false);

        dialog.Content = new StackPanel()
            .Padding(16)
            .Spacing(10)
            .Children(
                new Label().Text(title).FontSize(14),
                input,
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Children(confirm, cancel)
            );
        dialog.ShowDialog(_window!);
    }

    private void OnNavSelectionChanged(object? item)
    {
        if (item is CategoryTreeNode node)
        {
            ShowNav(node.Id);
            _workbench.OpenDocument("items");
        }
    }

    /// <summary>按导航节点显示编辑器区「启动项列表」:「全部」→ 所有项;「未分类」→ 无分类项;分类 id → 子树聚合。</summary>
    private void ShowNav(string navId)
    {
        _navId = navId;
        _listPanel.Clear();
        _listItems = [];
        _listButtons.Clear();
        _listBars.Clear();
        _styledSelection = -1;
        _hoveredIndex = -1;

        var aggregated = LauncherData.AggregateForNav(_store.Categories.ToList(), _items, navId);
        var shown = aggregated
            .Where(item => LauncherSearch.Matches(item, _query))
            .ToList();

        if (shown.Count == 0)
        {
            _listSelection.Clamp(0);
            var kind = EmptyState.ForList(
                hasItems: aggregated.Count > 0,
                hasQuery: !string.IsNullOrWhiteSpace(_query));
            _listPanel.Add(kind == EmptyStateKind.NoMatch
                ? EmptyStateView("无匹配启动项", "清空搜索", ClearItemsSearch,
                    _theme.EditorArea.Foreground, _theme.EditorArea.Background)
                : EmptyStateView("暂无启动项", "＋ 新增启动项", CreateItem,
                    _theme.EditorArea.Foreground, _theme.EditorArea.Background));
            return;
        }

        if (_viewMode == "list")
        {
            for (var i = 0; i < shown.Count; i++)
            {
                var (container, main, bar) = ListRow(shown[i], i);
                _listPanel.Add(container);
                _listItems.Add(shown[i]);
                _listButtons.Add(main);
                _listBars.Add(bar);
            }
        }
        else
        {
            var wrap = new WrapPanel { ItemWidth = 128, ItemHeight = 96, Spacing = 8 };
            for (var i = 0; i < shown.Count; i++)
            {
                var (container, main, bar) = Card(shown[i], i);
                wrap.Add(container);
                _listItems.Add(shown[i]);
                _listButtons.Add(main);
                _listBars.Add(bar);
            }

            _listPanel.Add(wrap);
        }

        _listSelection.Clamp(shown.Count);
        ApplyListSelection();
    }

    /// <summary>编辑器区列表行:图标 + 名称 + 命令,单击启动,悬停浮现「编辑」,右键菜单(编辑/删除)。</summary>
    private (UIElement Container, Button Main, Border Bar) ListRow(LauncherItem item, int index)
    {
        var icon = _icons.Resolve(item);
        var main = new Button()
            .Content(new StackPanel()
                .Orientation(Orientation.Horizontal)
                .Spacing(6)
                .Children(
                    IconElement(icon),
                    new StackPanel()
                        .Spacing(2)
                        .Children(
                            new Label().Text(item.Name)
                                .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground)),
                            new Label().Text(item.Command).FontSize(11)
                                .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground))
                        )
                ))
            .CanDrag(false)
            .WithTheme((_, button) => button.Background(_theme.EditorArea.Background));
        main.OnClick(() => TryLaunchFromList(item)); // 单击 → 启动(双击经防重只启动一次)
        AttachContextMenu(main, item);

        return BuildItemShell(item, main, index, editAtCorner: false);
    }

    /// <summary>卡片:大图标 + 名称,单击启动,悬停浮现「编辑」,右键菜单(编辑/删除)。</summary>
    private (UIElement Container, Button Main, Border Bar) Card(LauncherItem item, int index)
    {
        var icon = _icons.Resolve(item);
        var main = new Button()
            .Content(new StackPanel()
                .Orientation(Orientation.Vertical)
                .Spacing(6)
                .Children(
                    IconElement(icon, 40),
                    new Label().Text(item.Name)
                        .TextWrapping(TextWrapping.Wrap)
                        .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground))
                ))
            .CanDrag(false)
            .WithTheme((_, button) => button.Background(_theme.EditorArea.Background));
        main.OnClick(() => TryLaunchFromList(item)); // 单击 → 启动(双击经防重只启动一次)
        AttachContextMenu(main, item);
        return BuildItemShell(item, main, index, editAtCorner: true);
    }

    /// <summary>
    /// 组装列表项容器:主按钮(单击启动)+ 悬停浮现的「编辑」按钮 + 选中左缘条,三者兄弟叠加
    /// (不嵌套按钮,避免点击冲突);主按钮与编辑按钮共用悬停计数,悬停态在两者间移动不丢失。
    /// </summary>
    private (UIElement Container, Button Main, Border Bar) BuildItemShell(LauncherItem item, Button main, int index, bool editAtCorner)
    {
        var edit = new Button()
            .Content(new Label().Text("编辑")
                .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground)))
            .OnClick(() => EditItem(item))
            .CanDrag(false)
            .WithTheme((_, button) => button.Background(_theme.EditorArea.Background))
            .Padding(new Thickness(8, 2, 8, 2));
        edit.IsVisible = false; // 悬停时浮现
        edit.HorizontalAlignment = HorizontalAlignment.Right;
        edit.VerticalAlignment = editAtCorner ? VerticalAlignment.Top : VerticalAlignment.Center;

        var bar = new Border()
            .Width(3)
            .WithTheme((_, border) => border.Background(AccentBarColor));
        bar.IsVisible = false; // 选中时显示左缘条
        bar.HorizontalAlignment = HorizontalAlignment.Left;
        bar.VerticalAlignment = VerticalAlignment.Stretch;

        var hover = new HoverRefCount();
        hover.RaisedChanged += () =>
        {
            edit.IsVisible = hover.IsRaised;
            SetItemHovered(index, hover.IsRaised);
        };
        main.MouseEnter += () => hover.Enter();
        main.MouseLeave += () => hover.Leave();
        edit.MouseEnter += () => hover.Enter();
        edit.MouseLeave += () => hover.Leave();

        return (new Grid().Children(main, edit, bar), main, bar);
    }

    /// <summary>挂右键菜单(编辑 / 删除),右键时在鼠标位置弹出。</summary>
    private void AttachContextMenu(UIElement element, LauncherItem item)
    {
        var menu = new ContextMenu(new Menu()
            .Item("编辑", () => EditItem(item))
            .Separator()
            .Item("删除", () => DeleteItem(item)));
        element.MouseUp += e =>
        {
            if (e.RightButton)
            {
                menu.ShowAt(element, e.ScreenPosition);
            }
        };
    }

    private string ViewModeLabel() => _viewMode == "card" ? "卡片" : "列表";

    /// <summary>切换卡片/列表形态并持久化,立即重绘列表。</summary>
    private void ToggleViewMode()
    {
        _viewMode = _viewMode == "card" ? "list" : "card";
        var settings = _settings.Load();
        settings.ItemsViewMode = _viewMode;
        _settings.Save(settings);
        _modeToggleButton!.Content(new Label().Text(ViewModeLabel()));
        ShowNav(_navId);
    }

    /// <summary>编辑器区「启动项列表」文档:顶部工具栏(搜索 / 新增 / 形态切换)+ 聚合列表(可滚动)。</summary>
    private UIElement BuildItemsDocument()
    {
        var searchBox = new TextBox
        {
            Placeholder = "搜索启动项",
            CanDrag = false,
        };
        _itemsSearchBox = searchBox;
        searchBox.TextChanged += text =>
        {
            _query = text;
            ShowNav(_navId);
        };

        _modeToggleButton = new Button()
            .Content(new Label().Text(ViewModeLabel()))
            .ToolTip("切换卡片 / 列表")
            .OnClick(ToggleViewMode)
            .CanDrag(false);

        var scrollViewer = new ScrollViewer
        {
            Content = _listPanel,
            VerticalScroll = ScrollMode.Auto,
        };
        _listScrollViewer = scrollViewer;

        return new StackPanel()
            .Padding(12)
            .Spacing(8)
            .Children(
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Children(
                        searchBox,
                        new Button()
                            .Content(new Label().Text("＋ 新增"))
                            .ToolTip("新增启动项(归入当前分类)")
                            .OnClick(CreateItem)
                            .CanDrag(false),
                        _modeToggleButton
                    ),
                scrollViewer
            );
    }

    /// <summary>新建启动项:自动归入当前导航节点对应的分类(「全部」/「未分类」下新建归未分类)。</summary>
    private void CreateItem()
    {
        var item = new LauncherItem(
            "item-" + Guid.NewGuid().ToString("N")[..8],
            "新建启动项",
            "",
            CategoryId: LauncherData.CategoryIdForNewItem(_navId));

        _items.Add(item);
        _store.Save(_items);
        _itemHotkeys.RegisterAll();
        EditItem(item);
        ShowNav(_navId);
    }

    private UIElement BuildOutputPanel()
    {
        AppendLog("就绪");
        return _logPanel;
    }

    private void AppendLog(string message)
    {
        _logPanel.Add(new Label()
            .Text($"{DateTime.Now:HH:mm:ss}  {message}")
            .FontSize(12)
            .WithTheme((_, label) => label.Foreground(_theme.Panel.Foreground)));
    }

    /// <summary>列表单击启动入口:同一启动项在防重时间窗内(双击场景)只启动一次。</summary>
    private void TryLaunchFromList(LauncherItem item)
    {
        if (_launchDebouncer.ShouldLaunch(item.Id, DateTime.Now))
        {
            LaunchItem(item);
        }
    }

    /// <summary>列表键盘导航:移动选中并确保可见。</summary>
    private void MoveListSelection(int delta)
    {
        if (_listItems.Count == 0)
        {
            return;
        }

        if (delta < 0)
        {
            _listSelection.MoveUp(_listItems.Count);
        }
        else
        {
            _listSelection.MoveDown(_listItems.Count);
        }

        ApplyListSelection();
        EnsureSelectionVisible();
    }

    /// <summary>Enter 启动列表选中项(与单击共用防重入口)。</summary>
    private void LaunchSelected()
    {
        if (_listItems.Count == 0 || _listSelection.Selected >= _listItems.Count)
        {
            return;
        }

        TryLaunchFromList(_listItems[_listSelection.Selected]);
    }

    /// <summary>聚焦启动项列表搜索框(「/」或 Ctrl+F)。</summary>
    private void FocusItemsSearch() => _itemsSearchBox?.Focus();

    /// <summary>按当前选中索引重涂选中项为 accent,其余恢复区背景(仅重涂新旧两项,不重建列表)。</summary>
    private void ApplyListSelection()
    {
        if (_listButtons.Count == 0)
        {
            _styledSelection = -1;
            return;
        }

        var selected = _listSelection.Selected;
        if (_styledSelection == selected)
        {
            return;
        }

        if (_styledSelection >= 0 && _styledSelection < _listButtons.Count)
        {
            StyleListItem(_styledSelection, hovered: _styledSelection == _hoveredIndex);
        }

        if (selected >= 0 && selected < _listButtons.Count)
        {
            StyleListItem(selected, hovered: selected == _hoveredIndex);
        }

        _styledSelection = selected;
    }

    /// <summary>按「选中 &gt; 悬停 &gt; 常态」优先级重涂列表项:选中 = accent 背景 + 左缘条;悬停 = 背景微亮。</summary>
    private void StyleListItem(int index, bool hovered)
    {
        if (index < 0 || index >= _listButtons.Count)
        {
            return;
        }

        var button = _listButtons[index];
        var selected = index == _listSelection.Selected;
        button.Background(selected
            ? _theme.EditorArea.Accent
            : hovered ? HoverBackground : _theme.EditorArea.Background);

        if (index < _listBars.Count)
        {
            _listBars[index].IsVisible = selected; // 左缘条仅选中时显示
        }
    }

    /// <summary>悬停背景:暗主题向白微亮、亮主题向黑微暗(等效「升一层」)。</summary>
    private Color HoverBackground =>
        _theme.IsDark
            ? _theme.EditorArea.Background.Lerp(Color.FromRgb(255, 255, 255), 0.08)
            : _theme.EditorArea.Background.Lerp(Color.FromRgb(0, 0, 0), 0.06);

    /// <summary>选中左缘条:accent 向白提亮,保证 accent 背景上可见。</summary>
    private Color AccentBarColor => _theme.EditorArea.Accent.Lerp(Color.FromRgb(255, 255, 255), 0.45);

    /// <summary>记录当前悬停项并重涂:进入时替换旧悬停项,离开时仅当是当前悬停项才清除(事件顺序无关)。</summary>
    private void SetItemHovered(int index, bool raised)
    {
        if (raised)
        {
            if (_hoveredIndex == index)
            {
                return;
            }

            if (_hoveredIndex >= 0)
            {
                StyleListItem(_hoveredIndex, hovered: false);
            }

            _hoveredIndex = index;
            StyleListItem(index, hovered: true);
        }
        else if (_hoveredIndex == index)
        {
            _hoveredIndex = -1;
            StyleListItem(index, hovered: false);
        }
    }

    /// <summary>主题切换后全量重涂(WithTheme 回调先恢复区背景,此处重涂选中/悬停态)。</summary>
    private void ReapplyListSelection()
    {
        for (var i = 0; i < _listButtons.Count; i++)
        {
            StyleListItem(i, hovered: i == _hoveredIndex);
        }

        _styledSelection = _listSelection.Selected;
    }

    /// <summary>选中项滚出可视区时,滚动使其可见。</summary>
    private void EnsureSelectionVisible()
    {
        if (_listButtons.Count == 0 || _listScrollViewer is not { } scroller)
        {
            return;
        }

        var selected = _listButtons[_listSelection.Selected];
        var itemRect = selected.RectToScreen(new Rect(0, 0, selected.RenderSize.Width, selected.RenderSize.Height));
        var viewportRect = scroller.RectToScreen(new Rect(0, 0, scroller.ViewportWidth, scroller.ViewportHeight));

        if (itemRect.Top < viewportRect.Top)
        {
            scroller.SetScrollOffsets(scroller.HorizontalOffset, scroller.VerticalOffset - (viewportRect.Top - itemRect.Top));
        }
        else if (itemRect.Bottom > viewportRect.Bottom)
        {
            scroller.SetScrollOffsets(scroller.HorizontalOffset, scroller.VerticalOffset + (itemRect.Bottom - viewportRect.Bottom));
        }
    }

    /// <summary>焦点是否在文本输入控件内(搜索框/详情表单/下拉框):是则不劫持按键。</summary>
    private bool IsTextInputFocused() =>
        _window is { } window
        && window.FocusManager?.FocusedElement is TextBox or MultiLineTextBox or PasswordBox or ComboBox;

    private void LaunchItem(LauncherItem item)
    {
        var result = _runner.Launch(item);
        AppendLog(result.Success ? "✓ " + result.Message : "✗ " + result.Message);
        _launchStatus.Value = result.Message;
        // 失败醒目:状态栏红字 + toast;成功静默(仅状态栏轻文字)
        _workbench.SetStatusTextColor("launch", result.Success ? null : HotkeyWarning);
        if (!result.Success)
        {
            _window?.ShowToast(result.Message);
        }
    }

    private void Feedback(string message)
    {
        AppendLog("⚠ " + message);
        _launchStatus.Value = message;
    }

    private static UIElement IconElement(ImageSource? icon, int size = 16)
    {
        if (icon is null)
        {
            return new Border().Size(size, size);
        }

        return new Image().Source(icon).Size(size, size);
    }

    /// <summary>空状态组件:文案 + 引导动作按钮,启动项列表与分类树共用;颜色取自所在区的五区色板。</summary>
    private UIElement EmptyStateView(string message, string actionLabel, Action action, Color foreground, Color background) =>
        new StackPanel()
            .Padding(16)
            .Spacing(8)
            .Children(
                new Label().Text(message).FontSize(13)
                    .WithTheme((_, label) => label.Foreground(foreground)),
                new Button()
                    .Content(new Label().Text(actionLabel)
                        .WithTheme((_, label) => label.Foreground(foreground)))
                    .OnClick(action)
                    .CanDrag(false)
                    .WithTheme((_, button) => button.Background(background))
            );

    /// <summary>清空启动项列表搜索词并刷新列表。</summary>
    private void ClearItemsSearch()
    {
        _query = "";
        if (_itemsSearchBox is { } box)
        {
            box.Text = "";
        }

        ShowNav(_navId);
    }

    /// <summary>清空分类树搜索词并刷新树。</summary>
    private void ClearCategorySearch()
    {
        _sideBarFilters["launch"] = "";
        if (_categorySearchBox is { } box)
        {
            box.Text = "";
        }

        RefreshCategoryTree();
    }

    private void EditItem(LauncherItem item)
    {
        _current = item;
        ShowItemDetail(item);
        _workbench.SetDocumentReveal(DetailDocumentId, "launch", () => ShowNav(item.CategoryId ?? LauncherData.UncategorizedNavId));
        _workbench.OpenDocument(DetailDocumentId); // 编辑器区激活「启动项详情」文档
        _workbench.SetDocumentTitle(DetailDocumentId, item.Name); // 标签随当前对象显示项名
    }

    private void ShowEmptyDetail()
    {
        _current = null;
        _detailPanel.Clear();
        _detailPanel.Add(new Label()
            .Text("从左侧选择启动项查看详情")
            .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground)));
        _workbench.SetDocumentTitle(DetailDocumentId, "启动项详情"); // 无选中项时标签回落默认名
    }

    private void ShowItemDetail(LauncherItem item)
    {
        _detailPanel.Clear();
        _detailPanel.Add(BuildDetailForm(item));
    }

    private UIElement BuildDetailForm(LauncherItem item)
    {
        _loading = true;

        var name = TextField(item.Name, "启动项名称");
        var command = TextField(item.Command, "程序、脚本或 URL");
        var args = TextField(item.Args ?? "", "可选参数");
        var workingDirectory = TextField(item.WorkingDirectory ?? "", "可选工作目录");
        var categoryOptions = LauncherData.FlattenCategoryOptions(_store.Categories.ToList());
        var categorySource = new ItemsView<CategoryOption>(
            categoryOptions, option => option.Path, option => option.Id ?? "");
        var categoryIndex = categoryOptions.FindIndex(option => option.Id == item.CategoryId);
        var categoryCombo = new ComboBox
        {
            ItemsSource = categorySource,
            SelectedIndex = categoryIndex >= 0 ? categoryIndex : 0, // 0 = 未分类
            ChangeOnWheel = false,
            CanDrag = false,
        };
        categoryCombo.SelectionChanged += selected =>
        {
            if (selected is CategoryOption option)
            {
                UpdateCurrent(i => i with { CategoryId = option.Id });
            }
        };
        var icon = TextField(item.Icon ?? "", "可选图标路径");
        var hotkey = TextField(item.Hotkey ?? "", "可选每项热键,如 Ctrl+Shift+1");
        var hotkeyHint = new Label()
            .Text("")
            .FontSize(11)
            .WithTheme((_, label) => label.Foreground(HotkeyWarning));

        _loading = false;

        name.TextChanged += text => UpdateCurrent(i => i with { Name = text });
        command.TextChanged += text => UpdateCurrent(i => i with { Command = text });
        args.TextChanged += text => UpdateCurrent(i => i with { Args = string.IsNullOrWhiteSpace(text) ? null : text });
        workingDirectory.TextChanged += text => UpdateCurrent(i => i with { WorkingDirectory = string.IsNullOrWhiteSpace(text) ? null : text });
        categoryCombo.SelectionChanged += selected =>
        {
            if (selected is CategoryOption option)
            {
                UpdateCurrent(i => i with { CategoryId = option.Id });
            }
        };
        icon.TextChanged += text => UpdateCurrent(i => i with { Icon = string.IsNullOrWhiteSpace(text) ? null : text });
        hotkey.TextChanged += text =>
        {
            UpdateCurrent(i => i with { Hotkey = string.IsNullOrWhiteSpace(text) ? null : text });
            hotkeyHint.Text = text.Length > 0 && !HotkeyParser.TryParse(text, out _, out _)
                ? "热键格式无效,如 Ctrl+Shift+1(需至少一个修饰键)"
                : "";
        };

        return new StackPanel()
            .Padding(24)
            .Spacing(12)
            .Children(
                // 第一行工具行:启动 / 删除(编辑时测试启动不必滚动)
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Children(
                        new Button()
                            .Content(new Label().Text("启动"))
                            .OnClick(() => LaunchItem(item))
                            .CanDrag(false),
                        new Button()
                            .Content(new Label().Text("删除启动项"))
                            .OnClick(() => DeleteItem(item))
                            .CanDrag(false)
                    ),
                SectionTitle("基本", _theme.EditorArea.Foreground),
                FieldRow("名称", name),
                FieldRow("命令", command),
                FieldRow("参数", args),
                FieldRow("工作目录", workingDirectory),
                FieldRow("分类", categoryCombo),
                SectionTitle("高级", _theme.EditorArea.Foreground),
                FieldRow("图标", icon),
                FieldRow("每项热键", hotkey),
                hotkeyHint
            );
    }

    private UIElement FieldRow(string label, UIElement input) => new StackPanel()
        .Spacing(4)
        .Children(
            new Label().Text(label).FontSize(12)
                .WithTheme((_, l) => l.Foreground(_theme.EditorArea.Foreground)),
            input
        );

    /// <summary>详情页分区标题(基本/高级)。</summary>
    private static UIElement SectionTitle(string text, Color foreground) => new Label()
        .Text(text)
        .FontSize(14)
        .Bold()
        .WithTheme((_, label) => label.Foreground(foreground));

    private static TextBox TextField(string value, string placeholder) => new()
    {
        Text = value,
        Placeholder = placeholder,
        CanDrag = false,
    };

    private void UpdateCurrent(Func<LauncherItem, LauncherItem> edit)
    {
        if (_current is not { } current || _loading)
        {
            return;
        }

        var index = _items.FindIndex(item => item.Id == current.Id);
        if (index < 0)
        {
            return;
        }

        _current = edit(_items[index]);
        _items[index] = _current;
        _store.Save(_items);
        _itemHotkeys.RegisterAll();
        ShowNav(_navId);
    }

    private void DeleteItem(LauncherItem item)
    {
        _items.RemoveAll(candidate => candidate.Id == item.Id);
        _store.Save(_items);
        _itemHotkeys.RegisterAll();
        ShowEmptyDetail();
        ShowNav(_navId);
    }
}
