namespace MewPad.Core.Components.Card;

using Aprillz.MewUI.Controls;

/// <summary>
/// Represents a tab item definition for <see cref="Card"/>.
/// </summary>
public sealed class CardTabItem
{
    public CardTabItem(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the unique tab key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets the tab header element.
    /// </summary>
    public Element? Header { get; set; }

    /// <summary>
    /// Gets or sets the tab content element.
    /// </summary>
    public Element? Content { get; set; }

    /// <summary>
    /// Gets or sets whether the tab is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
