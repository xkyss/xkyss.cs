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
}

