using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using Mew.Launcher;
using Mew.Workbench;
using Icon = System.Drawing.Icon;
using WorkbenchType = Mew.Workbench.Workbench;

namespace Mew.Host;

/// <summary>
/// 宿主(Mew.Host.exe):唯一启动入口。组合根显式注册工具模块(编译期组合,ADR-000101-03/ADR-000200),
/// 独占 <see cref="Workbench.Build()"/>;窗口/托盘/标题栏/菜单/关于/主题循环/设置文档(外观节)/浮层生命周期归宿主。
/// 模块经 <see cref="IMewToolModule.Configure"/> 贡献五区、浮层搜索源与设置节;模块不组装、宿主不建领域视图。
/// </summary>
internal sealed class MewHost
{
    private const string AppVersion = "v0.2.0";
    private const string SettingsDocumentId = "settings-document";
    private const string SettingsAppearance = "appearance";
    private const string HotkeySectionId = "hotkey";
    private const string DefaultOverlayHotkey = "Ctrl+Alt+Space";
    private static readonly Color HotkeyWarning = Color.FromArgb(255, 200, 60, 60);

    private WorkbenchType _workbench = null!;
    private WorkbenchThemeContext _theme = null!;
    private SettingsService _settings = null!;
    private HotkeyService _hotkeys = null!;
    private OverlayWindow _overlayWindow = null!;
    private ToolModuleContext _context = null!;
    private Window _window = null!;
    private Button? _titleThemeButton;
    private Icon? _windowIcon;
    private IntPtr _windowLargeIcon;
    private IntPtr _windowSmallIcon;
    private List<RadioButton>? _themeRadios;
    private (ThemeVariant Mode, string Label)[] _themeModes =
    [
        (ThemeVariant.System, "跟随系统"),
        (ThemeVariant.Light, "亮色"),
        (ThemeVariant.Dark, "暗色"),
    ];
    private string _settingsNav = SettingsAppearance; // 设置上下文当前分类(外观 + 模块设置节)
    private StackPanel? _settingsContent;
    private string _overlayHotkey = null!; // 浮层呼出键(宿主热键),持久化于 settings.json 根节
    private bool _capturingHotkey;
    private Button? _hotkeyChangeButton;
    private Label? _hotkeyDisplay;
    private Label? _hotkeyHint;

    internal void Run()
    {
        var window = new NativeChromeWindow()
            .Title($"Mew Launcher — {AppVersion}")
            .Resizable(1080, 720);

        TrayIcon? tray = null;

        // 组装路径与窗口运行分离(USER STORY 15):Build() 前的部分不依赖消息循环,可在测试中无窗口复现
        var workbench = new WorkbenchType();
        var theme = workbench.ThemeContext;
        var overlay = new OverlayWindow(window, theme);
        var settings = new SettingsService();
        var settingsSections = new SettingsSectionRegistry();
        var hotkeys = new HotkeyService(); // 全局热键中央注册表:浮层呼出键 + 模块每项热键
        var context = new ToolModuleContext(
            workbench, window.Handle, window,
            hotkeys, settings, overlay, theme, settingsSections);

        _workbench = workbench;
        _theme = theme;
        _settings = settings;
        _hotkeys = hotkeys;
        _overlayWindow = overlay;
        _window = window;
        _context = context;

        settings.Load();
        _overlayHotkey = string.IsNullOrWhiteSpace(_settings.OverlayHotkey) ? DefaultOverlayHotkey : _settings.OverlayHotkey!;

        // 宿主设置节:热键(呼出键捕获/冲突检测)先于模块注册,保证设置侧边栏顺序 = 外观/热键/模块节
        settingsSections.Add(HotkeySectionId, "热键", BuildHotkeyPanel);

        // 浮层呼出键为宿主热键:先于模块注册,与每项热键冲突时按 v0.1.6 语义(浮层优先),失败反馈延迟到消息循环就绪
        var overlayHotkeyRegistered = _hotkeys.Register(window.Handle, _overlayHotkey, _overlayWindow.ShowOverlay);

        // 宿主主题(随设置持久化)先于模块视图构建生效
        workbench.Theme(themeContext => themeContext
            .SetMode(LoadThemeMode())
            .SetAccent(Accent.Blue));

        // 组合根:显式注册模块,全部贡献完后统一 Build()
        new LauncherModule().Configure(context);

        // 宿主贡献:设置上下文(活动栏/侧边栏/编辑器文档),设置节 = 外观/热键(宿主)+ 模块节(如 Launcher 的数据)
        workbench
            .ActivityBar(bar => bar.Item("settings", "设置", ActivityGlyph(SettingsIconData)))
            .SideBar(side => side.View("settings", "设置", BuildSettingsSideBar()))
            .EditorArea(editor => editor.Document(SettingsDocumentId, "设置", BuildSettingsDocument()));

        MigrateLegacySettingsDocumentLayout();

        _titleThemeButton = TitleBarBuilder.BuildTitleBar(window, Quit, OpenSettings, CycleTheme, workbench, ShowAbout);
        UpdateThemeButton(); // 初始图标/提示跟随已加载的主题模式

        // 主窗口按键:宿主热键捕获优先,再转发给订阅的模块(键盘导航);WM_HOTKEY 由中央注册表按 id 分发
        window.PreviewKeyDown += OnHostPreviewKeyDown;
        window.PreviewKeyDown += e => workbench.NotifyWindowKeyDown(e);

        window.Content = workbench.Build();

        window.Closing += e =>
        {
            e.Cancel = true;
            window.Hide();
        };

        window.Loaded += () =>
        {
            workbench.RefreshPresentation();
            ApplyWindowIcon(window);

            // 浮层呼出键启动注册失败(可能已被其他程序/模块占用)的反馈,延后到消息循环就绪再提示
            if (!overlayHotkeyRegistered)
            {
                window.ShowToast($"⚠ 呼出热键 {_overlayHotkey} 注册失败(可能已被其他程序占用)");
            }

            tray = new TrayIcon(window.Handle, ShowMain, Quit);
            tray.Add();

            // 主题模式变更(设置页 / 标题栏)统一持久化并同步各处显示
            if (Application.Current is { } app)
            {
                app.ThemeModeChanged += PersistThemeMode;
                app.ThemeModeChanged += SyncThemeRadios;
                app.ThemeModeChanged += UpdateThemeButton;
            }
        };

        window.NativeMessage += args =>
        {
            if (args is not Win32NativeMessageEventArgs e)
            {
                return;
            }

            if (e.Msg == HotkeyService.WmHotkey)
            {
                _hotkeys.Dispatch((int)e.WParam);
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
        DestroyWindowIcons();

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
        var stored = _settings.ThemeMode;
        return Enum.TryParse<ThemeVariant>(stored, out var mode) ? mode : ThemeVariant.System;
    }

    /// <summary>将当前主题模式持久化到 settings.json(保留其他设置字段)。</summary>
    private void PersistThemeMode()
    {
        _settings.ThemeMode = _theme.Mode.ToString();
        _settings.Save();
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

    /// <summary>标题栏齿轮与 File→设置:选择设置上下文并显式打开设置文档。</summary>
    private void OpenSettings()
    {
        _workbench.SelectActivity("settings");
        _workbench.OpenDocument(SettingsDocumentId);
    }

    /// <summary>设置分类内容:热键(呼出热键捕获改绑、冲突与格式提示;冲突检测走宿主中央热键服务)。</summary>
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

    /// <summary>进入热键捕获模式:下一次按键组合作为新呼出热键(Esc 取消);捕获在主窗口 PreviewKeyDown 前置处理。</summary>
    private void StartCaptureHotkey()
    {
        if (_capturingHotkey)
        {
            return;
        }

        _capturingHotkey = true;
        _hotkeyChangeButton!.Content(new Label().Text("请按下新热键…"));
        _hotkeyHint!.Text = "按 Esc 取消";
    }

    /// <summary>主窗口按键(先于模块转发):捕获模式下拦截组合键作为新呼出热键,并置 Handled 防止模块导航误触发。</summary>
    private void OnHostPreviewKeyDown(KeyEventArgs e)
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
        _hotkeyChangeButton!.Content(new Label().Text("更改"));
    }

    /// <summary>校验、冲突检测、重新注册并持久化新呼出热键;失败时保持原热键并在设置页提示。
    /// 冲突检测对全部注册热键生效(含启动项每项热键),改回当前值不算冲突。</summary>
    private void ApplyOverlayHotkey(string hotkey)
    {
        if (!HotkeyParser.TryParse(hotkey, out _, out _))
        {
            _hotkeyHint!.Text = "不支持的热键组合";
            return;
        }

        if (!string.Equals(hotkey, _overlayHotkey, StringComparison.OrdinalIgnoreCase)
            && _hotkeys.IsRegistered(hotkey))
        {
            _hotkeyHint!.Text = "与已注册热键冲突(含启动项每项热键),请换一个组合";
            return;
        }

        _hotkeys.Unregister(_overlayHotkey);
        if (!_hotkeys.Register(_window.Handle, hotkey, _overlayWindow.ShowOverlay))
        {
            _hotkeys.Register(_window.Handle, _overlayHotkey, _overlayWindow.ShowOverlay); // 恢复旧热键
            _hotkeyHint!.Text = "注册失败(可能已被其他程序占用)";
            return;
        }

        _overlayHotkey = hotkey;
        _hotkeyDisplay!.Text = hotkey;

        _settings.OverlayHotkey = hotkey;
        _settings.Save();

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

    /// <summary>设置窗口系统图标:大图标(任务栏/Alt+Tab)与小图标(标题栏/窗口切换)分别从 exe 图标资源提取,
    /// 避免 ExtractAssociatedIcon 只返回 32px 小图标导致任务栏放大后显小/模糊。</summary>
    private void ApplyWindowIcon(Window window)
    {
        var path = Environment.ProcessPath!;
        if (ExtractIconEx(path, 0, out var largeIcon, out var smallIcon, 1) > 0)
        {
            _windowLargeIcon = largeIcon;
            _windowSmallIcon = smallIcon;
            SendMessage(window.Handle, WmSetIcon, IconSmall, smallIcon);
            SendMessage(window.Handle, WmSetIcon, IconBig, largeIcon);
            return;
        }

        // 回退:资源提取失败时退回 ExtractAssociatedIcon
        _windowIcon = Icon.ExtractAssociatedIcon(path);
        if (_windowIcon is null)
        {
            return;
        }

        SendMessage(window.Handle, WmSetIcon, IconSmall, _windowIcon.Handle);
        SendMessage(window.Handle, WmSetIcon, IconBig, _windowIcon.Handle);
    }

    /// <summary>释放 ExtractIconEx 提取的窗口大/小图标句柄(WM_SETICON 不接管句柄所有权)。</summary>
    private void DestroyWindowIcons()
    {
        if (_windowLargeIcon != IntPtr.Zero)
        {
            DestroyIcon(_windowLargeIcon);
            _windowLargeIcon = IntPtr.Zero;
        }

        if (_windowSmallIcon != IntPtr.Zero)
        {
            DestroyIcon(_windowSmallIcon);
            _windowSmallIcon = IntPtr.Zero;
        }
    }

    private const uint WmSetIcon = 0x0080;
    private static readonly IntPtr IconSmall = IntPtr.Zero;
    private static readonly IntPtr IconBig = new(1);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint ExtractIconEx(string szFile, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIcons);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>活动栏图标按钮:PathShape 图标,填充随活动栏前景主题色(仿 Gallery SegmentIconShape)。</summary>
    private UIElement ActivityGlyph(string pathData)
    {
        var shape = new PathShape()
            .Stretch(Stretch.Uniform)
            .Width(18)
            .Height(18)
            .Center();
        shape.Data = PathGeometry.Parse(pathData);
        shape.WithTheme((_, s) => s.Fill = new SolidColorBrush(_theme.ActivityBar.Foreground));
        return shape;
    }

    /// <summary>编辑器区「设置」文档:内容容器,随侧边栏设置分类切换(外观 + 模块设置节)。</summary>
    private UIElement BuildSettingsDocument()
    {
        var content = new StackPanel();
        _settingsContent = content;
        ShowSettingsNav(_settingsNav);
        return content;
    }

    /// <summary>侧边栏「设置」上下文:外观(宿主保留节)+ 各模块经 SettingsSections 注册的设置节。</summary>
    private UIElement BuildSettingsSideBar() => new StackPanel()
        .Padding(12)
        .Spacing(4)
        .Children(
            new[] { SettingsNavButton("外观", SettingsAppearance) }
                .Concat(_context.SettingsSections.Sections.Select(section => SettingsNavButton(section.Label, section.Id)))
                .ToArray()
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
        content.Add(id == SettingsAppearance
            ? BuildAppearancePanel()
            : _context.SettingsSections.Build(id));
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

    private const string SettingsIconData =
        "M14 9.50006C11.5147 9.50006 9.5 11.5148 9.5 14.0001C9.5 16.4853 11.5147 18.5001 14 18.5001C15.3488 18.5001 16.559 17.9066 17.3838 16.9666C18.0787 16.1746 18.5 15.1365 18.5 14.0001C18.5 13.5401 18.431 13.0963 18.3028 12.6784C17.7382 10.8381 16.0253 9.50006 14 9.50006ZM11 14.0001C11 12.3432 12.3431 11.0001 14 11.0001C15.6569 11.0001 17 12.3432 17 14.0001C17 15.6569 15.6569 17.0001 14 17.0001C12.3431 17.0001 11 15.6569 11 14.0001Z M21.7093 22.3948L19.9818 21.6364C19.4876 21.4197 18.9071 21.4515 18.44 21.7219C17.9729 21.9924 17.675 22.4693 17.6157 23.0066L17.408 24.8855C17.3651 25.273 17.084 25.5917 16.7055 25.682C14.9263 26.1061 13.0725 26.1061 11.2933 25.682C10.9148 25.5917 10.6336 25.273 10.5908 24.8855L10.3834 23.0093C10.3225 22.4731 10.0112 21.9976 9.54452 21.7281C9.07783 21.4586 8.51117 21.4269 8.01859 21.6424L6.29071 22.4009C5.93281 22.558 5.51493 22.4718 5.24806 22.1859C4.00474 20.8536 3.07924 19.2561 2.54122 17.5137C2.42533 17.1384 2.55922 16.7307 2.8749 16.4977L4.40219 15.3703C4.83721 15.0501 5.09414 14.5415 5.09414 14.0007C5.09414 13.4598 4.83721 12.9512 4.40162 12.6306L2.87529 11.5051C2.55914 11.272 2.42513 10.8638 2.54142 10.4882C3.08038 8.74734 4.00637 7.15163 5.24971 5.82114C5.51684 5.53528 5.93492 5.44941 6.29276 5.60691L8.01296 6.36404C8.50793 6.58168 9.07696 6.54881 9.54617 6.27415C10.0133 6.00264 10.3244 5.52527 10.3844 4.98794L10.5933 3.11017C10.637 2.71803 10.9245 2.39704 11.3089 2.31138C12.19 2.11504 13.0891 2.01071 14.0131 2.00006C14.9147 2.01047 15.8128 2.11485 16.6928 2.31149C17.077 2.39734 17.3643 2.71823 17.4079 3.11017L17.617 4.98937C17.7116 5.85221 18.4387 6.50572 19.3055 6.50663C19.5385 6.507 19.769 6.45838 19.9843 6.36294L21.7048 5.60568C22.0626 5.44818 22.4807 5.53405 22.7478 5.81991C23.9912 7.1504 24.9172 8.74611 25.4561 10.487C25.5723 10.8623 25.4386 11.2703 25.1228 11.5035L23.5978 12.6297C23.1628 12.95 22.9 13.4586 22.9 13.9994C22.9 14.5403 23.1628 15.0489 23.5988 15.3698L25.1251 16.4965C25.441 16.7296 25.5748 17.1376 25.4586 17.5131C24.9198 19.2536 23.9944 20.8492 22.7517 22.1799C22.4849 22.4657 22.0671 22.5518 21.7093 22.3948ZM16.263 22.1966C16.4982 21.4685 16.9889 20.8288 17.6884 20.4238C18.5702 19.9132 19.6536 19.8547 20.5841 20.2627L21.9281 20.8526C22.791 19.8538 23.4593 18.7013 23.8981 17.4552L22.7095 16.5778L22.7086 16.5771C21.898 15.98 21.4 15.0277 21.4 13.9994C21.4 12.9719 21.8974 12.0195 22.7073 11.4227L22.7085 11.4218L23.8957 10.545C23.4567 9.2988 22.7881 8.14636 21.9248 7.1477L20.5922 7.73425L20.5899 7.73527C20.1844 7.91463 19.7472 8.00722 19.3039 8.00663C17.6715 8.00453 16.3046 6.77431 16.1261 5.15465L16.1259 5.15291L15.9635 3.69304C15.3202 3.57328 14.6677 3.50872 14.013 3.50017C13.3389 3.50891 12.6821 3.57367 12.0377 3.69328L11.8751 5.15452C11.7625 6.16272 11.1793 7.05909 10.3019 7.56986C9.41937 8.0856 8.34453 8.14844 7.40869 7.73694L6.07273 7.14893C5.20949 8.14751 4.54092 9.29983 4.10196 10.5459L5.29181 11.4233C6.11115 12.0269 6.59414 12.9837 6.59414 14.0007C6.59414 15.0173 6.11142 15.9742 5.29237 16.5776L4.10161 17.4566C4.54002 18.7044 5.2085 19.8585 6.07205 20.8587L7.41742 20.2682C8.34745 19.8613 9.41573 19.9215 10.2947 20.4292C11.174 20.937 11.7593 21.832 11.8738 22.84L11.8744 22.8445L12.0362 24.3088C13.3326 24.5638 14.6662 24.5638 15.9626 24.3088L16.1247 22.8418C16.1491 22.6217 16.1955 22.4055 16.263 22.1966Z";
}
