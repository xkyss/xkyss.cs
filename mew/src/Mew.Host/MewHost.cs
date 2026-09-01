using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;
using Mew.Launcher;
using Mew.Workbench;
using Mew.Workbench.Plugins;
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
    private (ThemeVariant Mode, string Label, string Icon, string ToolTip)[] _themeModes =
    [
        (ThemeVariant.System, "跟随系统", "🌓", "跟随系统 · 点击切换"),
        (ThemeVariant.Light, "亮色", "☀", "浅色 · 点击切换"),
        (ThemeVariant.Dark, "暗色", "☾", "暗色 · 点击切换"),
    ];
    private string _settingsNav = SettingsAppearance; // 设置上下文当前分类(外观 + 模块设置节)
    private StackPanel? _settingsContent;
    private string _overlayHotkey = null!; // 浮层呼出键(宿主热键),持久化于 settings.json 根节
    private bool _capturingHotkey;
    private Button? _hotkeyChangeButton;
    private Label? _hotkeyDisplay;
    private Label? _hotkeyHint;
    private PluginEnableStore _pluginEnables = null!;
    private IReadOnlyList<PluginDescriptor> _discoveredPlugins = [];
    private StackPanel? _pluginPanel;

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

        // 插件发现(票据 01):扫描用户目录与安装目录，校验清单与启用态
        _pluginEnables = new PluginEnableStore();
        _pluginEnables.Load();
        var discovery = new PluginDiscovery();
        var userPluginsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mew", "Plugins");
        var installPluginsDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        _discoveredPlugins = discovery.Discover(userPluginsDir, installPluginsDir);

        // 宿主设置节:热键(呼出键捕获/冲突检测)先于模块注册,保证设置侧边栏顺序 = 外观/热键/插件/模块节
        settingsSections.Add(HotkeySectionId, "热键", BuildHotkeyPanel);
        settingsSections.Add("plugins", "插件", BuildPluginPanel);

        // 浮层呼出键为宿主热键:先于模块注册,与每项热键冲突时按 v0.1.6 语义(浮层优先),失败反馈延迟到消息循环就绪
        var overlayHotkeyRegistered = _hotkeys.Register(window.Handle, _overlayHotkey, _overlayWindow.ShowOverlay, "浮层呼出键");

        // 宿主主题(随设置持久化)先于模块视图构建生效
        workbench.Theme(themeContext => themeContext
            .SetMode(LoadThemeMode())
            .SetAccent(Accent.Blue));

        // 组合根:显式注册模块(编译期组合,ADR-000101-03/ADR-000200),全部贡献完后统一 Build()
        AddModule(new LauncherModule());

        // 宿主贡献:设置上下文(活动栏/侧边栏/编辑器文档),设置节 = 外观/热键(宿主)+ 模块节(如 Launcher 的数据)
        workbench
            .ActivityBar(bar => bar.Item("settings", "设置", ShellIcons.ActivityGlyph(_theme, ShellIcons.SettingsIconData)))
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

    /// <summary>组合根:显式注册工具模块(编译期组合,ADR-000101-03/ADR-000200)。
    /// 模块经 <see cref="IMewToolModule.Configure"/> 经 ToolModuleContext 贡献五区/搜索源/设置节,注册后统一 Build。</summary>
    private void AddModule(IMewToolModule module) => module.Configure(_context);

    /// <summary>循环切换主题模式(跟随系统 → 亮 → 暗),持久化与标题栏图标由 ThemeModeChanged 统一处理。</summary>
    private void CycleTheme()
    {
        var index = Array.FindIndex(_themeModes, entry => entry.Mode == _theme.Mode);
        var next = _themeModes[(index + 1) % _themeModes.Length].Mode;
        _theme.SetMode(next);
    }

    /// <summary>标题栏主题按钮:图标/提示跟随当前模式(☀ 浅色 / ☾ 暗色 / 🌓 跟随系统)。</summary>
    private void UpdateThemeButton()
    {
        if (_titleThemeButton is not { } button)
        {
            return;
        }

        var entry = Array.Find(_themeModes, candidate => candidate.Mode == _theme.Mode);
        button.Content(new Label().Text(entry.Icon).FontSize(14));
        button.ToolTip(entry.ToolTip);
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
            .WithTheme((_, label) => label.Foreground(ShellIcons.HotkeyWarning));

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

        var keyName = HotkeyKeys.NameOf(e.Key);
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
            // 与 v0.1.6 一致:冲突提示点名占用方(浮层键/启动项每项热键),而非通用文案
            var owner = _hotkeys.FindOwner(hotkey);
            _hotkeyHint!.Text = owner is null
                ? "与已注册热键冲突,请换一个组合"
                : $"与{owner}的已注册热键冲突,请换一个组合";
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

    /// <summary>设置分类内容:插件（发现列表、健康态、启用开关、单 AOT 下 DLL 置灰）。</summary>
    private UIElement BuildPluginPanel()
    {
        var theme = _theme;
        var panel = new StackPanel().Padding(24).Spacing(12);
        _pluginPanel = panel;
        RefreshPluginPanel();
        return panel;
    }

    private void RefreshPluginPanel()
    {
        if (_pluginPanel is null) return;
        var theme = _theme;
        _pluginPanel.Clear();
        _pluginPanel.Add(new Label().Text("插件").FontSize(20).Bold()
            .WithTheme((_, l) => l.Foreground(theme.EditorArea.Foreground)));
        // 单 AOT 回退：当前扩展主机不可用，DLL 需 JIT
        const bool isJitAvailable = false;
        if (_discoveredPlugins.Count == 0)
        {
            _pluginPanel.Add(new Label().Text("未发现插件（将 plugin.json 置于 %APPDATA%/Mew/Plugins/<id>/）")
                .FontSize(12).WithTheme((_, l) => l.Foreground(theme.EditorArea.Foreground)));
            return;
        }
        foreach (var desc in _discoveredPlugins)
        {
            var health = desc.Health(isJitAvailable);
            var enabled = _pluginEnables.IsEnabled(desc.Id);
            var healthText = health switch
            {
                PluginHealth.Healthy => enabled ? "已启用" : "已禁用",
                PluginHealth.InvalidManifest => "清单错误",
                PluginHealth.DuplicateId => "ID 重复",
                PluginHealth.NeedsJit => "需 JIT 扩展主机",
                _ => health.ToString()
            };
            var row = new StackPanel().Orientation(Orientation.Horizontal).Spacing(8);
            var title = new Label().Text($"{desc.Manifest.DisplayName} ({desc.Id}) v{desc.Manifest.Version}")
                .WithTheme((_, l) => l.Foreground(health == PluginHealth.InvalidManifest || health == PluginHealth.DuplicateId ? ShellIcons.HotkeyWarning : theme.EditorArea.Foreground));
            var healthLabel = new Label().Text(healthText).FontSize(11)
                .WithTheme((_, l) => l.Foreground(health == PluginHealth.Healthy ? theme.EditorArea.Foreground : ShellIcons.HotkeyWarning));
            var toggle = new Button()
                .Content(new Label().Text(enabled ? "禁用" : "启用"))
                .CanDrag(false)
                .OnClick(() =>
                {
                    _pluginEnables.SetEnabled(desc.Id, !enabled);
                    _pluginEnables.Save();
                    RefreshPluginPanel();
                });
            // 置灰：清单错误或重复 ID 时禁用切换，需 JIT 时也禁用
            if (health == PluginHealth.InvalidManifest || health == PluginHealth.DuplicateId || health == PluginHealth.NeedsJit)
            {
                toggle.Content(new Label().Text(healthText));
            }
            if (!string.IsNullOrWhiteSpace(desc.ValidationErrors.FirstOrDefault()))
            {
                var err = new Label().Text(string.Join("; ", desc.ValidationErrors)).FontSize(11)
                    .WithTheme((_, l) => l.Foreground(ShellIcons.HotkeyWarning));
                _pluginPanel.Add(new StackPanel().Spacing(2).Children(title, healthLabel, err, toggle));
            }
            else
            {
                _pluginPanel.Add(new StackPanel().Spacing(2).Children(title, healthLabel, toggle));
            }
        }
    }
}
