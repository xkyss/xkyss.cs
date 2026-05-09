namespace MewPad.Core.Services.Impl;

using MewPad.Core.Interfaces;
using MewPad.Core.Services;

internal class SettingsService : ISettingsService
{
    private readonly List<ISettingsCategory> _categories = [];

    public IReadOnlyList<ISettingsCategory> Categories => _categories;

    public void RegisterCategory(ISettingsCategory category)
    {
        if (!_categories.Any(c => c.Id == category.Id))
            _categories.Add(category);
    }

    public ISettingsCategory? GetCategory(string id)
        => _categories.FirstOrDefault(c => c.Id == id);

    private Action<string>? _openHandler;

    public void SetOpenHandler(Action<string> handler) => _openHandler = handler;

    public void OpenSettings(string categoryId = "appearance") => _openHandler?.Invoke(categoryId);
}