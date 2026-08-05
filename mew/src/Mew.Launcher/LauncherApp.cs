using System.ComponentModel;
using System.Diagnostics;
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
    private const string AppVersion = "v0.1.3";
    private const string DefaultOverlayHotkey = "Ctrl+Alt+Space";
    private static readonly Color HotkeyWarning = Color.FromArgb(255, 200, 60, 60);

    private readonly LauncherStore _store = new();
    private readonly SettingsStore _settings = new();
    private readonly ObservableValue<string> _hotkeyStatus;
    private string _overlayHotkey;
    private Window? _window;
    private bool _capturingHotkey;
    private Button? _hotkeyChangeButton;
    private Label? _hotkeyDisplay;
    private Label? _hotkeyHint;
    private readonly LauncherRunner _runner = new();
    private readonly IconResolver _icons = new();
    private readonly ObservableValue<string> _launchStatus = new("就绪");
    private readonly List<LauncherItem> _items;
    private readonly ItemHotkeys _itemHotkeys;
    private readonly WorkbenchType _workbench = new();
    private readonly WorkbenchThemeContext _theme;
    private readonly StackPanel _listPanel = new();
    private readonly StackPanel _detailPanel = new();
    private readonly StackPanel _logPanel = new();
    private string _navId = LauncherData.AllNavId; // 当前导航节点:「全部」/ 分类 id /「未分类」
    private LauncherItem? _current;
    private bool _loading;
    private TreeView? _tree;
    private string _viewMode = "card"; // 启动项列表形态:card(卡片,默认)/ list(列表),持久化于 settings.json
    private string _query = "";
    private Button? _modeToggleButton;
    private string _categoryQuery = "";
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

        BuildTitleBar(window, Quit, OpenSettings);

        _workbench
            .Theme(theme => theme
                .SetMode(LoadThemeMode())
                .SetAccent(Accent.Blue))
            .ActivityBar(bar =>
            {
                bar.Item("launch", "启动", GlyphKind.Hamburger, ShowLaunchContext);
                bar.Item("settings", "设置", SettingsGlyph(), ShowSettingsContext);
            })
            .SideBar(side => side
                .View("launch", "启动", BuildCategoryTree())
                .View("settings", "设置", BuildSettingsSideBar()))
            .EditorArea(editor => editor
                .Document("items", "启动项", BuildItemsDocument())
                .Document("detail", "启动项详情", _detailPanel)
                .Document("settings", "设置", BuildSettingsDocument()))
            .Panel(panel => panel.View("output", "输出", BuildOutputPanel()))
            .StatusBar(status => status
                .Item("launch", _launchStatus)
                .Item("shortcut", _hotkeyStatus));

        ShowNav(_navId);
        ShowEmptyDetail();

        window.Content = _workbench.Build();

        var overlay = new OverlayWindow(window, _items, _runner, _icons, _theme);

        window.Closing += e =>
        {
            e.Cancel = true;
            window.Hide();
        };

        window.Loaded += () =>
        {
            if (!GlobalHotkey.Register(window.Handle, _overlayHotkey))
            {
                AppendLog($"⚠ 呼出热键 {_overlayHotkey} 注册失败(可能已被其他程序占用)");
            }

            _itemHotkeys.Attach(window.Handle);
            _itemHotkeys.RegisterAll();
            tray = new TrayIcon(window.Handle, ShowMain, Quit);
            tray.Add();

            // 主题模式变更(状态栏循环按钮 / 设置页)统一持久化并同步设置页单选(票据 03)
            if (Application.Current is { } app)
            {
                app.ThemeModeChanged += PersistThemeMode;
                app.ThemeModeChanged += SyncThemeRadios;
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

    /// <summary>标题栏:左区图标 + 菜单栏(File=设置/退出、Help=关于)、右区设置齿轮入口。</summary>
    private static void BuildTitleBar(NativeChromeWindow window, Action quit, Action openSettings)
    {
        var appIcon = IconResolver.ExtractIcon(Environment.ProcessPath!);
        if (appIcon is not null)
        {
            window.TitleBarLeft.Add(new Image()
                .Source(appIcon)
                .Size(16, 16)
                .Margin(new Thickness(8, 0, 4, 0)));
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
                new MenuItem("_Help").Menu(
                    new Menu().Item("关于", () => ShowAbout(window)))
            );

        window.TitleBarLeft.Add(menuBar);

        // 右区:设置入口(齿轮),与 File→设置 打开同一设置页
        var settingsButton = new Button()
            .Content(new Label().Text("⚙").FontSize(14))
            .ToolTip("设置")
            .OnClick(openSettings)
            .CanDrag(false)
            .Size(36, 28);
        window.TitleBarRight.Add(settingsButton);
    }

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

    /// <summary>活动栏「启动」上下文:侧边栏切分类树、编辑器区切启动项列表。</summary>
    private void ShowLaunchContext()
    {
        _workbench.OpenToolPane("launch");
        _workbench.OpenDocument("items");
    }

    /// <summary>活动栏「设置」上下文:侧边栏切设置分类,编辑器区显示对应设置内容。</summary>
    private void ShowSettingsContext()
    {
        _workbench.OpenToolPane("settings");
        _workbench.OpenDocument("settings");
    }

    private UIElement SettingsGlyph() => new Label()
        .Text("⚙")
        .FontSize(14)
        .WithTheme((_, label) => label.Foreground(_theme.ActivityBar.Foreground));

    /// <summary>标题栏齿轮与 File→设置:切到「设置」上下文。</summary>
    private void OpenSettings() => ShowSettingsContext();

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
            CanDrag = false,
        };
        searchBox.TextChanged += text =>
        {
            _categoryQuery = text;
            RefreshCategoryTree();
        };

        var tree = new TreeView
        {
            SelectionMode = ItemsSelectionMode.Single,
            ExpandTrigger = TreeViewExpandTrigger.ClickChevron,
            CanDrag = false,
        };
        _tree = tree;
        tree.SelectionChanged += OnNavSelectionChanged;
        tree.MouseUp += OnCategoryTreeRightClick;
        RefreshCategoryTree(); // 构建树项并默认选中「全部」

        return new StackPanel()
            .Padding(12)
            .Spacing(6)
            .Children(
                new Label()
                    .Text("启动项")
                    .FontSize(12)
                    .WithTheme((_, label) => label.Foreground(_theme.SideBar.Foreground)),
                searchBox,
                tree
            );
    }

    /// <summary>(重新)构建分类树项:按分类搜索词过滤,并默认选中「全部」。</summary>
    private void RefreshCategoryTree()
    {
        _treeItems = new TreeItemsView<CategoryTreeNode>(
            LauncherData.FilterNavTree(
                LauncherData.BuildNavTree(_store.Categories.ToList()), _categoryQuery),
            node => node.Children,
            node => node.Name,
            node => node.Id,
            node => node.Children.Count > 0);
        _tree!.ItemsSource = _treeItems;
        _treeItems.SelectSingle(0);
    }

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
        }
    }

    /// <summary>按导航节点显示编辑器区「启动项列表」:「全部」→ 所有项;「未分类」→ 无分类项;分类 id → 子树聚合。</summary>
    private void ShowNav(string navId)
    {
        _navId = navId;
        _listPanel.Clear();

        var shown = LauncherData.AggregateForNav(_store.Categories.ToList(), _items, navId)
            .Where(item => LauncherSearch.Matches(item, _query))
            .ToList();

        if (shown.Count == 0)
        {
            _listPanel.Add(EmptyListLabel());
            return;
        }

        if (_viewMode == "list")
        {
            foreach (var item in shown)
            {
                _listPanel.Add(ListRow(item));
            }

            return;
        }

        var wrap = new WrapPanel { ItemWidth = 128, ItemHeight = 96, Spacing = 8 };
        foreach (var item in shown)
        {
            wrap.Add(Card(item));
        }

        _listPanel.Add(wrap);
    }

    /// <summary>编辑器区列表行:图标 + 名称 + 命令,双击启动,行尾「启动」按钮。</summary>
    private UIElement ListRow(LauncherItem item)
    {
        var icon = _icons.Resolve(item);
        var rowButton = new Button()
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
            .WithTheme((_, button) => button.Background(_theme.EditorArea.Background))
            .Column(0);
        rowButton.OnClick(() => EditItem(item)); // 单击 → 打开详情
        rowButton.MouseDoubleClick += _ => LaunchItem(item); // 双击 → 启动
        AttachContextMenu(rowButton, item);

        return new Grid()
            .Columns("*,Auto")
            .Children(
                rowButton,
                new Button()
                    .Content(new Label()
                        .Text("启动")
                        .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground)))
                    .OnClick(() => LaunchItem(item))
                    .CanDrag(false)
                    .Column(1)
            );
    }

    /// <summary>卡片:大图标 + 名称,单击开详情、双击启动、右键菜单(编辑/删除)。</summary>
    private UIElement Card(LauncherItem item)
    {
        var icon = _icons.Resolve(item);
        var card = new Button()
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
        card.OnClick(() => EditItem(item));
        card.MouseDoubleClick += _ => LaunchItem(item);
        AttachContextMenu(card, item);
        return card;
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
                new ScrollViewer
                {
                    Content = _listPanel,
                    VerticalScroll = ScrollMode.Auto,
                }
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

    private void LaunchItem(LauncherItem item)
    {
        var result = _runner.Launch(item);
        AppendLog(result.Success ? "✓ " + result.Message : "✗ " + result.Message);
        _launchStatus.Value = result.Message;
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

    private UIElement EmptyListLabel() => new Label()
        .Text(string.IsNullOrWhiteSpace(_query) ? "暂无启动项" : "无匹配启动项")
        .FontSize(12)
        .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground));

    private void EditItem(LauncherItem item)
    {
        _current = item;
        ShowItemDetail(item);
        _workbench.OpenDocument("detail"); // 编辑器区激活「启动项详情」文档
    }

    private void ShowEmptyDetail()
    {
        _current = null;
        _detailPanel.Clear();
        _detailPanel.Add(new Label()
            .Text("从左侧选择启动项查看详情")
            .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground)));
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
                new Label().Text(item.Name).FontSize(20).Bold()
                    .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground)),
                new Label().Text(item.Id).FontSize(11)
                    .WithTheme((_, label) => label.Foreground(_theme.EditorArea.Foreground)),
                FieldRow("名称", name),
                FieldRow("命令", command),
                FieldRow("参数", args),
                FieldRow("工作目录", workingDirectory),
                FieldRow("分类", categoryCombo),
                FieldRow("图标", icon),
                FieldRow("每项热键", hotkey),
                hotkeyHint,
                new Button()
                    .Content(new Label().Text("启动"))
                    .OnClick(() => LaunchItem(item))
                    .CanDrag(false),
                new Button()
                    .Content(new Label().Text("删除启动项"))
                    .OnClick(() => DeleteItem(item))
                    .CanDrag(false)
            );
    }

    private UIElement FieldRow(string label, UIElement input) => new StackPanel()
        .Spacing(4)
        .Children(
            new Label().Text(label).FontSize(12)
                .WithTheme((_, l) => l.Foreground(_theme.EditorArea.Foreground)),
            input
        );

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
