using System.Reflection;

namespace Ks.Core.Utilities.System.IO;

public static class Pathx
{
    public static string RuntimeDirectory()
    {
        return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Directory.GetCurrentDirectory();
    }
}
