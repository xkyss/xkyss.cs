namespace ComponentsDemo.Plugin.UI.Demos;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using MewPad.Core.Components.Card;
using MewPad.Core.Interfaces;

/// <summary>
/// Content tab demonstrating all Card component variants.
/// </summary>
internal sealed class CardDemoContent : IContentItem
{
    public string Id => "components.demo.card";
    public string Title => "Card 卡片";
    public object? Icon => "🃏";

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
            Spacing = 24,
            Margin = new Thickness(24),
        };

        root.Add(BuildSection("基础用法", BuildBasicCards()));
        root.Add(BuildSection("尺寸", BuildSizeCards()));
        root.Add(BuildSection("Hoverable（可悬停）", BuildHoverableCards()));
        root.Add(BuildSection("Loading（加载中）", BuildLoadingCard()));
        root.Add(BuildSection("无边框 Borderless", BuildBorderlessCard()));
        root.Add(BuildSection("Inner 类型（嵌套卡片）", BuildInnerCard()));
        root.Add(BuildSection("内置 Tabs", BuildTabsCard()));
        root.Add(BuildSection("Actions 操作区", BuildActionsCard()));
        root.Add(BuildSection("CardMeta", BuildMetaCard()));
        root.Add(BuildSection("CardGrid", BuildCardGridDemo()));

        scroll.Content = root;
        return scroll;
    }

    // ── Sections ─────────────────────────────────────────────────────────

    private static FrameworkElement BuildSection(string title, FrameworkElement content)
    {
        var section = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        var heading = new Label
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
        };
        section.Add(heading);
        section.Add(content);
        return section;
    }

    // ── Demos ─────────────────────────────────────────────────────────────

    private static FrameworkElement BuildBasicCards()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };

        // 无标题
        var plain = new Card { Width = 200 }
            .Body(new Label { Text = "纯内容，无标题。", FontSize = 12 });

        // 带标题和 Extra
        var withHeader = new Card { Width = 200 }
            .Title("卡片标题")
            .Extra(new Button { Content = new Label { Text = "更多", FontSize = 11 } })
            .Body(new Label { Text = "这是卡片的正文内容区域。", FontSize = 12 });

        row.Add(plain);
        row.Add(withHeader);
        return row;
    }

    private static FrameworkElement BuildSizeCards()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };

        var small = new Card { Width = 200 }
            .Title("Small 尺寸")
            .Size(CardSize.Small)
            .Body(new Label { Text = "内边距更紧凑。", FontSize = 12 });

        var medium = new Card { Width = 200 }
            .Title("Medium 尺寸（默认）")
            .Size(CardSize.Medium)
            .Body(new Label { Text = "标准内边距。", FontSize = 12 });

        row.Add(small);
        row.Add(medium);
        return row;
    }

    private static FrameworkElement BuildHoverableCards()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };

        var hoverable = new Card { Width = 200 }
            .Title("Hoverable")
            .Hoverable()
            .Body(new Label { Text = "鼠标悬停 / 点击有视觉反馈。", FontSize = 12 })
            .OnClick(() => { /* demo click */ });

        var normal = new Card { Width = 200 }
            .Title("普通（不可交互）")
            .Body(new Label { Text = "无悬停效果。", FontSize = 12 });

        row.Add(hoverable);
        row.Add(normal);
        return row;
    }

    private static FrameworkElement BuildLoadingCard()
    {
        return new Card { Width = 240 }
            .Title("加载中")
            .Loading();
    }

    private static FrameworkElement BuildBorderlessCard()
    {
        return new Card { Width = 240 }
            .Title("Borderless")
            .Variant(CardVariant.Borderless)
            .Body(new Label { Text = "无边框变体，适合融入背景的场景。", FontSize = 12 });
    }

    private static FrameworkElement BuildInnerCard()
    {
        var inner = new Card { Width = 200 }
            .Title("内嵌卡片")
            .Type(CardType.Inner)
            .Body(new Label { Text = "使用 Inner 类型，内边距更小。", FontSize = 12 });

        return new Card { Width = 280 }
            .Title("外层卡片")
            .Body(inner);
    }

    private static FrameworkElement BuildTabsCard()
    {
        return new Card { Width = 320 }
            .Title("内置 Tabs")
            .Tabs(
                new CardTabItem("tab1") { Header = new Label { Text = "Tab 1" }, Content = new Label { Text = "Tab 1 的内容。", FontSize = 12 } },
                new CardTabItem("tab2") { Header = new Label { Text = "Tab 2" }, Content = new Label { Text = "Tab 2 的内容。", FontSize = 12 } },
                new CardTabItem("tab3") { Header = new Label { Text = "禁用", }, IsEnabled = false }
            )
            .OnTabChange(key => { /* key changed */ });
    }

    private static FrameworkElement BuildActionsCard()
    {
        var likeBtn = new Button { Content = new Label { Text = "👍 点赞", FontSize = 12 } };
        var shareBtn = new Button { Content = new Label { Text = "↗ 分享", FontSize = 12 } };

        return new Card { Width = 280 }
            .Title("操作区")
            .Body(new Label { Text = "卡片底部自适应排列操作按钮。", FontSize = 12 })
            .Actions(likeBtn, shareBtn)
            .OnAction(idx => { /* action index */ });
    }

    private static FrameworkElement BuildMetaCard()
    {
        var avatar = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = 20,
            Background = default,
            Child = new Label { Text = "👤", FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
        };
        avatar.WithTheme((t, b) => b.Background = t.Palette.Accent.WithAlpha(40));

        var meta = new CardMeta()
            .Avatar(avatar)
            .Title("用户名称")
            .Description("用户的一段描述文字，可以换行。");

        return new Card { Width = 280 }
            .Body(meta);
    }

    private static FrameworkElement BuildCardGridDemo()
    {
        var grid = new Grid();
        grid.Columns("*, *");

        var g1 = new CardGrid
        {
            Padding = new Thickness(16),
            BorderThickness = 1,
            Content = new Label { Text = "网格项 1 — 可点击", FontSize = 12 },
        };
        g1.Clicked += () => { /* clicked */ };

        var g2 = new CardGrid
        {
            Padding = new Thickness(16),
            BorderThickness = 1,
            Content = new Label { Text = "网格项 2 — 可点击", FontSize = 12 },
        };
        g2.Clicked += () => { /* clicked */ };

        Grid.SetColumn(g1, 0);
        Grid.SetColumn(g2, 1);
        grid.Add(g1);
        grid.Add(g2);

        return new Card { Width = 400 }
            .Title("CardGrid 网格布局")
            .Body(grid);
    }
}
