using System.Runtime.InteropServices;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mew.Workbench;

namespace Mew.Launcher;

/// <summary>
/// 全局热键呼出的悬浮搜索浮层:独立于主窗口,输入即过滤(复用 LauncherSearch)、
/// 回车启动选中项、上下键切换、Esc/失焦关闭;与窗口内管理共用启动项数据、执行器与图标解析。
/// </summary>
internal sealed class OverlayWindow
{
    private const int MaxResults = 8;
    private const int Width = 640;
    private const int Height = 420;
    private static readonly Color White = Color.FromArgb(255, 255, 255, 255);

    private readonly Window _window;
    private readonly Window _owner;
    private readonly List<LauncherItem> _items;
    private readonly LauncherRunner _runner;
    private readonly IconResolver _icons;
    private readonly WorkbenchThemeContext _theme;
    private readonly TextBox _searchBox = new() { Placeholder = "输入以搜索启动项", CanDrag = false };
    private readonly StackPanel _resultPanel = new();
    private List<LauncherItem> _results = [];
    private int _selected;

    internal OverlayWindow(Window owner, List<LauncherItem> items, LauncherRunner runner, IconResolver icons, WorkbenchThemeContext theme)
    {
        _owner = owner;
        _items = items;
        _runner = runner;
        _icons = icons;
        _theme = theme;

        _window = new Window
        {
            Borderless = true,
            Topmost = true,
            ShowInTaskbar = false,
        };

        _window.Content = BuildContent();
        _window.PreviewKeyDown += OnKeyDown;
        _searchBox.TextChanged += text => Refresh(text);
        _window.Deactivated += () => _window.Hide();
    }

    internal void ShowOverlay()
    {
        _window.Show(_owner);
        PositionOverlay();
        _window.Activate();
        _searchBox.Text = "";
        _searchBox.Focus();
        Refresh("");
    }

    private UIElement BuildContent() => new Border()
        .WithTheme((_, border) => border.Background(_theme.EditorArea.Background))
        .Child(
            new StackPanel()
                .Padding(16)
                .Spacing(8)
                .Children(
                    _searchBox,
                    _resultPanel
                )
        );

    private void PositionOverlay()
    {
        var screenWidth = User32.GetSystemMetrics(0); // SM_CXSCREEN
        var screenHeight = User32.GetSystemMetrics(1); // SM_CYSCREEN
        var x = (screenWidth - Width) / 2;
        var y = Math.Max(40, (screenHeight - Height) / 5);
        User32.SetWindowPos(_window.Handle, User32.HwndTopmost, x, y, Width, Height, 0x0040);
    }

    private void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                _window.Hide();
                e.Handled = true;
                break;

            case Key.Enter when _results.Count > 0:
                Launch(_results[_selected]);
                e.Handled = true;
                break;

            case Key.Up when _results.Count > 0:
                _selected = (_selected - 1 + _results.Count) % _results.Count;
                Refresh(_searchBox.Text);
                e.Handled = true;
                break;

            case Key.Down when _results.Count > 0:
                _selected = (_selected + 1) % _results.Count;
                Refresh(_searchBox.Text);
                e.Handled = true;
                break;
        }
    }

    private void Refresh(string query)
    {
        _results = _items.Where(item => LauncherSearch.Matches(item, query)).Take(MaxResults).ToList();
        _selected = _results.Count == 0 ? 0 : Math.Clamp(_selected, 0, _results.Count - 1);

        _resultPanel.Clear();
        for (var i = 0; i < _results.Count; i++)
        {
            _resultPanel.Add(Row(_results[i], i == _selected));
        }
    }

    private UIElement Row(LauncherItem item, bool selected)
    {
        var icon = _icons.Resolve(item);

        return new Button()
            .Content(
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(8)
                    .Children(
                        IconElement(icon),
                        new StackPanel()
                            .Spacing(2)
                            .Children(
                                new Label().Text(item.Name)
                                    .WithTheme((_, label) => label.Foreground(selected ? White : _theme.EditorArea.Foreground)),
                                new Label().Text(item.Command).FontSize(11)
                                    .WithTheme((_, label) => label.Foreground(selected ? White : _theme.EditorArea.Foreground))
                            )
                    ))
            .OnClick(() => Launch(item))
            .CanDrag(false)
            .WithTheme((_, button) => button.Background(selected ? _theme.EditorArea.Accent : _theme.EditorArea.Background));
    }

    private void Launch(LauncherItem item)
    {
        _runner.Launch(item);
        _window.Hide();
    }

    private static UIElement IconElement(ImageSource? icon)
    {
        if (icon is null)
        {
            return new Border().Size(16, 16);
        }

        return new Image().Source(icon).Size(16, 16);
    }

    private static class User32
    {
        internal static readonly IntPtr HwndTopmost = new(-1);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(int nIndex);
    }
}
