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
    private const string AllCategory = "全部";

    private readonly LauncherStore _store = new();
    private readonly List<LauncherItem> _items;
    private readonly WorkbenchType _workbench = new();
    private readonly WorkbenchThemeContext _theme;
    private readonly StackPanel _listPanel = new();
    private readonly StackPanel _detailPanel = new();
    private string _category = AllCategory;
    private string _query = "";
    private LauncherItem? _current;
    private bool _loading;

    internal LauncherApp()
    {
        _items = _store.Load();
        _theme = _workbench.ThemeContext;
    }

    internal void Run()
    {
        var window = new Window()
            .Title("Mew Launcher — v0.1.1")
            .Resizable(1080, 720);

        var categories = _items.Select(item => item.Category).Distinct().ToList();

        _workbench
            .Theme(theme => theme
                .SetMode(ThemeVariant.System)
                .SetAccent(Accent.Blue))
            .ActivityBar(bar =>
            {
                bar.Item("all", AllCategory, GlyphKind.Hamburger, () => ShowCategory(AllCategory));
                foreach (var category in categories)
                {
                    bar.Item(category, category, GlyphKind.Plus, () => ShowCategory(category));
                }
            })
            .SideBar(side => side.View("launcher", "启动项", BuildSideBar()))
            .EditorArea(editor => editor.Document("detail", "启动项详情", _detailPanel))
            .Panel(panel => panel.View("output", "输出", BuildOutputPanel()))
            .StatusBar(status => status
                .Item("ready", "就绪")
                .Item("shortcut", "Ctrl+Alt+Space"));

        ShowCategory(AllCategory);
        ShowEmptyDetail();

        window.Content = _workbench.Build();

        Application.Run(window);
    }

    private UIElement BuildSideBar()
    {
        var searchBox = new TextBox
        {
            Placeholder = "搜索启动项",
            CanDrag = false,
        };
        searchBox.TextChanged += text =>
        {
            _query = text;
            ShowCategory(_category);
        };

        return new StackPanel()
            .Padding(12)
            .Spacing(6)
            .Children(
                new Button()
                    .Content(new Label()
                        .Text("＋ 新增启动项")
                        .WithTheme((_, label) => label.Foreground(_theme.SideBar.Foreground)))
                    .OnClick(CreateItem)
                    .CanDrag(false)
                    .WithTheme((_, button) => button.Background(_theme.SideBar.Background)),
                searchBox,
                _listPanel
            );
    }

    private UIElement BuildOutputPanel() => new StackPanel()
        .Padding(12)
        .Children(
            new Label().Text("就绪")
                .WithTheme((_, label) => label.Foreground(_theme.Panel.Foreground))
        );

    private void ShowCategory(string category)
    {
        _category = category;
        _listPanel.Clear();

        var shown = (category == AllCategory
                ? _items
                : _items.Where(item => item.Category == category))
            .Where(item => LauncherSearch.Matches(item, _query))
            .ToList();

        if (shown.Count == 0)
        {
            _listPanel.Add(EmptyListLabel());
            return;
        }

        foreach (var item in shown)
        {
            _listPanel.Add(SideBarRow(item));
        }
    }

    private UIElement SideBarRow(LauncherItem item) => new Button()
        .Content(
            new StackPanel()
                .Spacing(2)
                .Children(
                    new Label().Text(item.Name)
                        .WithTheme((_, label) => label.Foreground(_theme.SideBar.Foreground)),
                    new Label().Text(item.Command).FontSize(11)
                        .WithTheme((_, label) => label.Foreground(_theme.SideBar.Foreground))
                ))
        .OnClick(() => EditItem(item))
        .CanDrag(false);

    private UIElement EmptyListLabel() => new Label()
        .Text(string.IsNullOrWhiteSpace(_query) ? "暂无启动项" : "无匹配启动项")
        .FontSize(12)
        .WithTheme((_, label) => label.Foreground(_theme.SideBar.Foreground));

    private void EditItem(LauncherItem item)
    {
        _current = item;
        ShowItemDetail(item);
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
        var category = TextField(item.Category, "分类");
        var icon = TextField(item.Icon ?? "", "可选图标路径");
        var hotkey = TextField(item.Hotkey ?? "", "可选每项热键,如 Ctrl+Shift+1");

        _loading = false;

        name.TextChanged += text => UpdateCurrent(i => i with { Name = text });
        command.TextChanged += text => UpdateCurrent(i => i with { Command = text });
        args.TextChanged += text => UpdateCurrent(i => i with { Args = string.IsNullOrWhiteSpace(text) ? null : text });
        workingDirectory.TextChanged += text => UpdateCurrent(i => i with { WorkingDirectory = string.IsNullOrWhiteSpace(text) ? null : text });
        category.TextChanged += text => UpdateCurrent(i => i with { Category = string.IsNullOrWhiteSpace(text) ? AllCategory : text });
        icon.TextChanged += text => UpdateCurrent(i => i with { Icon = string.IsNullOrWhiteSpace(text) ? null : text });
        hotkey.TextChanged += text => UpdateCurrent(i => i with { Hotkey = string.IsNullOrWhiteSpace(text) ? null : text });

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
                FieldRow("分类", category),
                FieldRow("图标", icon),
                FieldRow("每项热键", hotkey),
                new Button()
                    .Content(new Label().Text("删除启动项"))
                    .OnClick(() => DeleteItem(item))
                    .CanDrag(false)
            );
    }

    private UIElement FieldRow(string label, TextBox input) => new StackPanel()
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
        ShowCategory(_category);
    }

    private void CreateItem()
    {
        var category = _category == AllCategory ? "默认" : _category;
        var item = new LauncherItem(
            "item-" + Guid.NewGuid().ToString("N")[..8],
            "新建启动项",
            "",
            Category: category);

        _items.Add(item);
        _store.Save(_items);
        EditItem(item);
        ShowCategory(_category);
    }

    private void DeleteItem(LauncherItem item)
    {
        _items.RemoveAll(candidate => candidate.Id == item.Id);
        _store.Save(_items);
        ShowEmptyDetail();
        ShowCategory(_category);
    }
}
