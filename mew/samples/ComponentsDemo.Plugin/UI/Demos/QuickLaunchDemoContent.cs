namespace ComponentsDemo.Plugin.UI.Demos;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Components.Card;
using MewPad.Core.Interfaces;

/// <summary>
/// Content tab demonstrating Card usage for quick-launch scenarios:
/// bookmarked websites and local software shortcuts.
/// </summary>
internal sealed class QuickLaunchDemoContent : IContentItem
{
    public string Id => "components.demo.quicklaunch";
    public string Title => "QuickLaunch 快速启动";
    public object? Icon => "🚀";

    public FrameworkElement CreateContent()
    {
        var scroll = new ScrollViewer
        {
            VerticalScroll = ScrollMode.Auto,
            HorizontalScroll = ScrollMode.Disabled,
        };

        var root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 28,
            Margin = new Thickness(24),
        };

        root.Add(BuildSection("网址收藏", BuildUrlLauncher()));
        root.Add(BuildSection("本地软件", BuildAppLauncher()));
        root.Add(BuildSection("紧凑网格（混合）", BuildCompactGrid()));

        scroll.Content = root;
        return scroll;
    }

    // ── Section wrapper ───────────────────────────────────────────────────

    private static FrameworkElement BuildSection(string title, FrameworkElement content)
    {
        var section = new StackPanel { Orientation = Orientation.Vertical, Spacing = 10 };
        section.Add(new Label { Text = title, FontSize = 14, FontWeight = FontWeight.SemiBold });
        section.Add(content);
        return section;
    }

    // ── URL launcher ─────────────────────────────────────────────────────
    // Each site is its own hoverable Card.

    private static FrameworkElement BuildUrlLauncher()
    {
        (string Icon, string Name, string Url, string Desc)[] sites =
        [
            ("🔍", "Google",      "https://google.com",             "全球最大搜索引擎"),
            ("📦", "GitHub",      "https://github.com",             "开源代码托管平台"),
            ("📖", "MDN",         "https://developer.mozilla.org",  "Web 技术参考文档"),
            ("🤖", "ChatGPT",     "https://chat.openai.com",        "AI 对话助手"),
            ("📰", "Hacker News", "https://news.ycombinator.com",   "技术资讯社区"),
            ("🗺️", "百度地图",    "https://map.baidu.com",          "中国地图服务"),
        ];

        // Two rows of three cards each
        var col1 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new Thickness(0, 0, 0, 0) };
        var col2 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        for (var i = 0; i < sites.Length; i++)
        {
            var (icon, name, url, desc) = sites[i];
            var card = BuildUrlCard(icon, name, url, desc);
            (i < 3 ? col1 : col2).Add(card);
        }

        var container = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12 };
        container.Add(col1);
        container.Add(col2);
        return container;
    }

    private static FrameworkElement BuildUrlCard(string icon, string name, string url, string desc)
    {
        // Body: large icon on the left, desc + url stacked on the right
        var iconLabel = new Label
        {
            Text = icon,
            FontSize = 32,
            Width = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };

        var descLabel = new Label
        {
            Text = desc,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Bottom,
        };
        descLabel.WithTheme((t, l) => l.Foreground = t.Palette.DisabledText);

        var urlLabel = new Label
        {
            Text = url,
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Top,
        };
        urlLabel.WithTheme((t, l) => l.Foreground = t.Palette.DisabledText);

        var info = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
        info.Add(descLabel);
        info.Add(urlLabel);

        var bodyRow = new DockPanel();
        DockPanel.SetDock(iconLabel, Dock.Left);
        bodyRow.Add(iconLabel);
        bodyRow.Add(info);

        // Category as Extra
        var catExtra = new Label { Text = "网址", FontSize = 10 };
        catExtra.WithTheme((t, l) => l.Foreground = t.Palette.Accent);

        var openBtn = new Button { Content = new Label { Text = "打开", FontSize = 11 } };
        openBtn.Click += () => { /* shell.LaunchUrl(url) */ };
        var editBtn = new Button { Content = new Label { Text = "编辑", FontSize = 11 } };

        return new Card { Width = 260 }
            .Title(name)
            .Extra(catExtra)
            .Size(CardSize.Small)
            .Hoverable()
            .Body(bodyRow)
            .Actions(openBtn, editBtn);
    }

    // ── App launcher ─────────────────────────────────────────────────────
    // Each local app is its own Card with a launch action button.

    private static FrameworkElement BuildAppLauncher()
    {
        (string Icon, string Name, string Path, string Category)[] apps =
        [
            ("🖥️", "VS Code",          @"C:\Users\...\Code.exe",                        "开发工具"),
            ("🦊", "Firefox",           @"C:\Program Files\...\firefox.exe",             "浏览器"),
            ("🎵", "网易云音乐",        @"C:\Program Files\...\cloudmusic.exe",          "娱乐"),
            ("📁", "Total Commander",   @"C:\totalcmd\TOTALCMD64.EXE",                   "文件管理"),
            ("🐳", "Docker Desktop",    @"C:\Program Files\Docker\...\Docker Desktop.exe","开发工具"),
            ("🔒", "KeePassXC",         @"C:\...\KeePassXC.exe",                         "安全"),
        ];

        var col1 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var col2 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        for (var i = 0; i < apps.Length; i++)
        {
            var (icon, name, path, category) = apps[i];
            var card = BuildAppCard(icon, name, path, category);
            (i < 3 ? col1 : col2).Add(card);
        }

        var container = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12 };
        container.Add(col1);
        container.Add(col2);
        return container;
    }

    private static FrameworkElement BuildAppCard(string icon, string name, string path, string category)
    {
        // Body: large icon on the left, desc stub + path stacked on the right
        var iconLabel = new Label
        {
            Text = icon,
            FontSize = 32,
            Width = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };

        var descLabel = new Label
        {
            Text = $"{name} 的快捷方式",
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Bottom,
        };
        descLabel.WithTheme((t, l) => l.Foreground = t.Palette.DisabledText);

        var pathLabel = new Label
        {
            Text = path,
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Top,
        };
        pathLabel.WithTheme((t, l) => l.Foreground = t.Palette.DisabledText);

        var info = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
        info.Add(descLabel);
        info.Add(pathLabel);

        var bodyRow = new DockPanel();
        DockPanel.SetDock(iconLabel, Dock.Left);
        bodyRow.Add(iconLabel);
        bodyRow.Add(info);

        // Category as Extra
        var catExtra = new Label { Text = category, FontSize = 10 };
        catExtra.WithTheme((t, l) => l.Foreground = t.Palette.Accent);

        var openBtn = new Button { Content = new Label { Text = "打开", FontSize = 11 } };
        openBtn.Click += () => { /* Process.Start(path) */ };
        var editBtn = new Button { Content = new Label { Text = "编辑", FontSize = 11 } };

        return new Card { Width = 260 }
            .Title(name)
            .Extra(catExtra)
            .Size(CardSize.Small)
            .Hoverable()
            .Body(bodyRow)
            .Actions(openBtn, editBtn);
    }

    // ── Compact grid (mixed) ──────────────────────────────────────────────
    // 4-column launchpad — each item is its own small Card.

    private static FrameworkElement BuildCompactGrid()
    {
        (string Icon, string Name)[] items =
        [
            ("🌐", "浏览器"),  ("📧", "邮件"),   ("📅", "日历"),  ("🗒️", "记事本"),
            ("🖩",  "计算器"), ("🎨", "画图"),   ("🎬", "播放器"),("📊", "表格"),
            ("⚙️", "设置"),   ("🔍", "搜索"),   ("🗂️", "文件"),  ("🖥️",  "终端"),
        ];

        var rows = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12 };
        for (var rowStart = 0; rowStart < items.Length; rowStart += 4)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            for (var j = rowStart; j < rowStart + 4 && j < items.Length; j++)
            {
                var (icon, name) = items[j];

                var iconL = new Label
                {
                    Text = icon,
                    FontSize = 26,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                var body = new StackPanel { Orientation = Orientation.Vertical, Spacing = 2, HorizontalAlignment = HorizontalAlignment.Center };
                body.Add(iconL);

                var card = new Card { Width = 100 }
                    .Title(name)
                    .Size(CardSize.Small)
                    .Hoverable()
                    .Body(body)
                    .OnClick(() => { /* launch */ });

                row.Add(card);
            }
            rows.Add(row);
        }
        return rows;
    }
}
