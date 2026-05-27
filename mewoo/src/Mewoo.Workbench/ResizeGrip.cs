using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace Mewoo.Workbench;

internal enum ResizeGripOrientation
{
    Vertical,
    Horizontal,
}

internal static class ResizeGrip
{
    public static Border Create(ResizeGripOrientation orientation, Action<double> onDelta)
    {
        var grip = new Border
        {
            Background = Color.FromRgb(0, 0, 0).WithAlpha(0),
        };

        if (orientation == ResizeGripOrientation.Vertical)
        {
            grip.Width = 4;
            grip.MinWidth = 4;
        }
        else
        {
            grip.Height = 4;
            grip.MinHeight = 4;
        }

        var dragging = false;
        var start = default(Point);

        grip.MouseEnter += () =>
        {
            grip.Background = Application.Current.Theme.Palette.Accent.WithAlpha(80);
        };

        grip.MouseLeave += () =>
        {
            if (!dragging)
            {
                grip.Background = Color.FromRgb(0, 0, 0).WithAlpha(0);
            }
        };

        grip.MouseDown += e =>
        {
            if (e.Handled || e.Button != MouseButton.Left)
            {
                return;
            }

            dragging = true;
            start = e.GetPosition((UIElement)grip.FindVisualRoot()!);

            if (grip.FindVisualRoot() is Window window)
            {
                window.CaptureMouse(grip);
            }

            e.Handled = true;
        };

        grip.MouseMove += e =>
        {
            if (!dragging)
            {
                return;
            }

            var current = e.GetPosition((UIElement)grip.FindVisualRoot()!);
            var delta = orientation == ResizeGripOrientation.Vertical
                ? current.X - start.X
                : start.Y - current.Y;

            onDelta(delta);
        };

        grip.MouseUp += _ =>
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            grip.Background = Color.FromRgb(0, 0, 0).WithAlpha(0);

            if (grip.FindVisualRoot() is Window window)
            {
                window.ReleaseMouseCapture();
            }
        };

        return grip;
    }

    public static Border CreateVerticalAbsolute(Func<double> getStartValue, Action<double> onValue, Action? onCompleted = null)
    {
        var grip = new Border
        {
            Width = 6,
            MinWidth = 6,
            Background = Color.FromRgb(0, 0, 0).WithAlpha(0),
        };

        var dragging = false;
        var start = default(Point);
        var startValue = 0.0;

        grip.MouseEnter += () =>
        {
            grip.Background = Application.Current.Theme.Palette.Accent.WithAlpha(70);
        };

        grip.MouseLeave += () =>
        {
            if (!dragging)
            {
                grip.Background = Color.FromRgb(0, 0, 0).WithAlpha(0);
            }
        };

        grip.MouseDown += e =>
        {
            if (e.Handled || e.Button != MouseButton.Left)
            {
                return;
            }

            var root = grip.FindVisualRoot() as UIElement;
            if (root is null)
            {
                return;
            }

            dragging = true;
            start = e.GetPosition(root);
            startValue = getStartValue();

            if (grip.FindVisualRoot() is Window window)
            {
                window.CaptureMouse(grip);
            }

            e.Handled = true;
        };

        grip.MouseMove += e =>
        {
            if (!dragging)
            {
                return;
            }

            var root = grip.FindVisualRoot() as UIElement;
            if (root is null)
            {
                return;
            }

            var current = e.GetPosition(root);
            onValue(startValue + current.X - start.X);
            e.Handled = true;
        };

        grip.MouseUp += _ =>
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            grip.Background = Color.FromRgb(0, 0, 0).WithAlpha(0);

            if (grip.FindVisualRoot() is Window window)
            {
                window.ReleaseMouseCapture();
            }

            onCompleted?.Invoke();
        };

        return grip;
    }
}
