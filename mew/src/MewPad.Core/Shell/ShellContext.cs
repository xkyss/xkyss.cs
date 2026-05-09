namespace MewPad.Core.Shell;

using Aprillz.MewUI.Controls;
using MewPad.Core.Interfaces;
using MewPad.Core.Services;
using MewPad.Core.Services.Impl;

/// <summary>
/// Global context container that manages all shell state and services.
/// This is the main hub for the entire application framework.
/// </summary>
public class ShellContext
{
    // === Collections for registered items ===
    private readonly List<IActivityItem> _activities = [];
    private readonly List<IPanelItem> _panels = [];
    private readonly Dictionary<string, IContentItem> _contentItems = [];
    private readonly Dictionary<string, StatusBarItem> _statusBarItems = [];

    // === Observable state ===
    /// <summary>The currently active activity ID.</summary>
    public ObservableValue<string?> ActiveActivityId { get; } = new(null);

    /// <summary>Whether the SideBar is collapsed.</summary>
    public ObservableValue<bool> SideBarCollapsed { get; } = new(false);

    /// <summary>Whether the PanelArea is collapsed.</summary>
    public ObservableValue<bool> PanelCollapsed { get; } = new(false);

    /// <summary>The currently active panel ID.</summary>
    public ObservableValue<string?> ActivePanelId { get; } = new(null);

    /// <summary>The currently active content tab ID.</summary>
    public ObservableValue<string?> ActiveContentId { get; } = new(null);

    // === Global services ===
    /// <summary>Theme service for Light/Dark mode.</summary>
    public IThemeService Theme { get; }

    /// <summary>Localization service for multi-language support.</summary>
    public ILocalizationService Localization { get; }

    /// <summary>Settings service for category management.</summary>
    public ISettingsService Settings { get; }

    /// <summary>Configuration service for persistence.</summary>
    public IConfigurationService Configuration { get; }

    /// <summary>
    /// Create a new shell context with default or custom services.
    /// </summary>
    public ShellContext(
        IThemeService? themeService = null,
        ILocalizationService? localizationService = null,
        ISettingsService? settingsService = null,
        IConfigurationService? configurationService = null)
    {
        Theme = themeService ?? new ThemeService();
        Localization = localizationService ?? new LocalizationService();
        Settings = settingsService ?? new SettingsService();
        Configuration = configurationService ?? new ConfigurationService();
    }

    // === Activity Management ===

    /// <summary>Register an activity item (e.g., Explorer, Search).</summary>
    public void RegisterActivity(IActivityItem item)
    {
        if (_activities.FirstOrDefault(a => a.Id == item.Id) != null)
            throw new InvalidOperationException($"Activity with id '{item.Id}' already registered");
        _activities.Add(item);
    }

    /// <summary>Get all registered activities, sorted by Order.</summary>
    public IReadOnlyList<IActivityItem> GetActivities()
        => _activities.OrderBy(a => a.Order).ToList();

    /// <summary>Get an activity by ID.</summary>
    public IActivityItem? GetActivity(string id)
        => _activities.FirstOrDefault(a => a.Id == id);

    // === Panel Management ===

    /// <summary>Register a panel item (e.g., Problems, Debug Console).</summary>
    public void RegisterPanel(IPanelItem item)
    {
        if (_panels.FirstOrDefault(p => p.Id == item.Id) != null)
            throw new InvalidOperationException($"Panel with id '{item.Id}' already registered");
        _panels.Add(item);
    }

    /// <summary>Get all registered panels, sorted by Order.</summary>
    public IReadOnlyList<IPanelItem> GetPanels()
        => _panels.OrderBy(p => p.Order).ToList();

    /// <summary>Get a panel by ID.</summary>
    public IPanelItem? GetPanel(string id)
        => _panels.FirstOrDefault(p => p.Id == id);

    // === Content Tab Management ===

    /// <summary>Open a content tab (or activate existing tab with same ID).</summary>
    public void OpenContent(IContentItem item)
    {
        if (!_contentItems.ContainsKey(item.Id))
        {
            _contentItems[item.Id] = item;
        }
        ActiveContentId.Value = item.Id;
    }

    /// <summary>Close a content tab by ID.</summary>
    public void CloseContent(string id)
    {
        _contentItems.Remove(id);
        if (ActiveContentId.Value == id)
        {
            ActiveContentId.Value = _contentItems.Keys.FirstOrDefault();
        }
    }

    /// <summary>Get a content item by ID.</summary>
    public IContentItem? GetContent(string id)
        => _contentItems.TryGetValue(id, out var item) ? item : null;

    /// <summary>Get all open content tabs.</summary>
    public IReadOnlyList<IContentItem> GetOpenContent()
        => _contentItems.Values.ToList();

    // === Status Bar Management ===

    /// <summary>Register a status bar item (e.g., file encoding, line numbers).</summary>
    public void RegisterStatusBarItem(StatusBarItem item)
    {
        _statusBarItems[item.Id] = item;
    }

    /// <summary>Get status bar items for a specific slot, ordered by priority.</summary>
    public IReadOnlyList<StatusBarItem> GetStatusBarItems(StatusBarSlot slot)
        => _statusBarItems.Values
            .Where(item => item.Slot == slot)
            .OrderBy(item => item.Priority)
            .ToList();
}

/// <summary>
/// Status bar item definition.
/// </summary>
public record StatusBarItem(
    string Id,
    Func<FrameworkElement> CreateElement,
    StatusBarSlot Slot = StatusBarSlot.Left,
    int Priority = 100);

/// <summary>
/// Status bar slot (left or right).
/// </summary>
public enum StatusBarSlot
{
    /// <summary>Left slot (content flows left-to-right).</summary>
    Left,

    /// <summary>Right slot (content flows right-to-left).</summary>
    Right
}