using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    private string PresentationFilePath => Path.Combine(Path.GetDirectoryName(FilePath)!, "presentation.json");

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

    public WorkbenchPresentationState? TryLoadPresentation()
    {
        try
        {
            return File.Exists(PresentationFilePath)
                ? JsonSerializer.Deserialize(File.ReadAllText(PresentationFilePath), WorkbenchJsonContext.Default.WorkbenchPresentationState)
                : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void SavePresentation(WorkbenchPresentationState state)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PresentationFilePath)!);
            File.WriteAllText(PresentationFilePath, JsonSerializer.Serialize(state, WorkbenchJsonContext.Default.WorkbenchPresentationState));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

internal sealed record WorkbenchPresentationState(string? ActiveActivityId, bool IsSideBarVisible);

[JsonSerializable(typeof(WorkbenchPresentationState))]
internal sealed partial class WorkbenchJsonContext : JsonSerializerContext;
