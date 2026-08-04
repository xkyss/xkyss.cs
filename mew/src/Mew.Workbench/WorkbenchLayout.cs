using System.IO;

namespace Mew.Workbench;

/// <summary>
/// Persists the dock layout as JSON under the per-user Mew application data folder.
/// </summary>
internal sealed class WorkbenchLayoutStore
{
    public WorkbenchLayoutStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        FilePath = Path.Combine(appData, "Mew", "layout.json");
    }

    public string FilePath { get; }

    public string? TryLoad()
    {
        try
        {
            return File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public void Save(string json)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, json);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
