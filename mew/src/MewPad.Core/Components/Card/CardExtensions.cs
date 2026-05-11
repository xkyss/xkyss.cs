namespace MewPad.Core.Components.Card;

using Aprillz.MewUI.Controls;

/// <summary>
/// Fluent API extension methods for <see cref="Card"/>, <see cref="CardMeta"/>, and <see cref="CardGrid"/>.
/// </summary>
public static class CardExtensions
{
    public static Card Title(this Card card, UIElement? title)
    {
        card.Title = title;
        return card;
    }

    public static Card Title(this Card card, string? text)
    {
        card.Title = text == null ? null : new Label { Text = text };
        return card;
    }

    public static Card Extra(this Card card, UIElement? extra)
    {
        card.Extra = extra;
        return card;
    }

    public static Card Cover(this Card card, UIElement? cover)
    {
        card.Cover = cover;
        return card;
    }

    public static Card Body(this Card card, UIElement? body)
    {
        card.Body = body;
        return card;
    }

    public static Card Loading(this Card card, bool loading = true)
    {
        card.Loading = loading;
        return card;
    }

    public static Card Hoverable(this Card card, bool hoverable = true)
    {
        card.Hoverable = hoverable;
        return card;
    }

    public static Card Variant(this Card card, CardVariant variant)
    {
        card.Variant = variant;
        return card;
    }

    public static Card Size(this Card card, CardSize size)
    {
        card.Size = size;
        return card;
    }

    public static Card Type(this Card card, CardType type)
    {
        card.Type = type;
        return card;
    }

    public static Card Bordered(this Card card, bool bordered = true)
    {
        card.Bordered = bordered;
        return card;
    }

    public static Card Tabs(this Card card, params CardTabItem[] tabs)
    {
        card.SetTabs(tabs);
        return card;
    }

    public static Card Tabs(this Card card, IEnumerable<CardTabItem> tabs)
    {
        card.SetTabs(tabs);
        return card;
    }

    public static Card Actions(this Card card, params Element[] actions)
    {
        card.SetActions(actions);
        return card;
    }

    public static Card Actions(this Card card, IEnumerable<Element> actions)
    {
        card.SetActions(actions);
        return card;
    }

    public static Card ActiveTabKey(this Card card, string? key)
    {
        card.ActiveTabKey = key;
        return card;
    }

    public static Card DefaultActiveTabKey(this Card card, string? key)
    {
        card.DefaultActiveTabKey = key;
        return card;
    }

    public static Card OnTabChange(this Card card, Action<string> handler)
    {
        card.TabChanged += handler;
        return card;
    }

    public static Card OnClick(this Card card, Action handler)
    {
        card.Clicked += handler;
        return card;
    }

    public static Card OnAction(this Card card, Action<int> handler)
    {
        card.ActionInvoked += handler;
        return card;
    }

    public static CardMeta Avatar(this CardMeta meta, Element? avatar)
    {
        meta.Avatar = avatar;
        return meta;
    }

    public static CardMeta Title(this CardMeta meta, Element? title)
    {
        meta.Title = title;
        return meta;
    }

    public static CardMeta Title(this CardMeta meta, string? text)
    {
        meta.Title = text == null ? null : new Label { Text = text };
        return meta;
    }

    public static CardMeta Description(this CardMeta meta, Element? description)
    {
        meta.Description = description;
        return meta;
    }

    public static CardMeta Description(this CardMeta meta, string? text)
    {
        meta.Description = text == null ? null : new Label { Text = text };
        return meta;
    }

    public static CardGrid Hoverable(this CardGrid grid, bool hoverable = true)
    {
        grid.Hoverable = hoverable;
        return grid;
    }

    public static CardGrid OnClick(this CardGrid grid, Action handler)
    {
        grid.Clicked += handler;
        return grid;
    }
}
