using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Aprillz.MewUI;

namespace Mew.Launcher;

/// <summary>
/// 启动项图标解析:自定义图标路径优先,否则提取命令对应可执行文件的图标;结果按路径缓存。
/// 窗口内列表与呼出浮层共用,保证显示一致。
/// </summary>
internal sealed class IconResolver
{
    private readonly Dictionary<string, ImageSource?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public ImageSource? Resolve(LauncherItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Icon))
        {
            return FromPath(item.Icon!);
        }

        var command = item.Command.Trim();
        if (command.Length == 0 || IsUrl(command))
        {
            return null;
        }

        var exePath = ResolveExePath(command);
        return exePath is null ? null : FromPath(exePath);
    }

    private ImageSource? FromPath(string path)
    {
        if (_cache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        ImageSource? result = null;
        try
        {
            if (IsImageFile(path))
            {
                result = ImageSource.FromFile(path);
            }
            else
            {
                using var icon = Icon.ExtractAssociatedIcon(path);
                if (icon is not null)
                {
                    using var bitmap = icon.ToBitmap();
                    var bgra = BitmapToBgra(bitmap);
                    result = ImageSource.FromBgraPixels(bitmap.Width, bitmap.Height, bgra, hasAlpha: true);
                }
            }
        }
        catch (Exception ex) when (ex is ArgumentException or FileNotFoundException or IOException or UnauthorizedAccessException)
        {
            result = null;
        }

        _cache[path] = result;
        return result;
    }

    private static bool IsImageFile(string path) =>
        Path.GetExtension(path).ToLowerInvariant() is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".ico" or ".webp";

    private static bool IsUrl(string command) =>
        command.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || command.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    private static string? ResolveExePath(string command)
    {
        if (File.Exists(command))
        {
            return command;
        }

        var extensions = new[] { ".exe", ".bat", ".cmd", ".com", ".lnk" };
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            var dir = directory.Trim();
            if (dir.Length == 0)
            {
                continue;
            }

            foreach (var extension in extensions)
            {
                var candidate = Path.Combine(dir, command + extension);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static byte[] BitmapToBgra(Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var bgra = new byte[width * height * 4];
        var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var stride = data.Stride;
            var rowBytes = width * 4;
            for (var y = 0; y < height; y++)
            {
                Marshal.Copy(data.Scan0 + y * stride, bgra, y * rowBytes, rowBytes);
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return bgra;
    }
}
