using System.Reflection;

namespace Ks.Core.Utilities.System.IO;

public static class Pathx
{
    public static string RuntimeDirectory()
    {
        return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Directory.GetCurrentDirectory();
    }

    public static void EnsureDirectoryExists(string directory)
    {
        // 检查目录是否存在
        if (!Directory.Exists(directory))
        {
            // 如果目录不存在，则创建它
            Directory.CreateDirectory(directory);
        }
    }

    public static string Profile(string project)
    {
        return Profile(KsConstants.ProfilePath, project);
    }

    public static string Profile(string xky, string project)
    {
        // 获取当前用户的用户文件夹路径
        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var specificPath = Path.Combine(path, xky, project);
        // 如果子文件夹不存在，则创建它
        if (!Directory.Exists(specificPath))
        {
            Directory.CreateDirectory(specificPath);
        }
        return specificPath;
    }
}
