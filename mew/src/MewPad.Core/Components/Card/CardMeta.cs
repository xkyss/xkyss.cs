namespace MewPad.Core.Components.Card;

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

/// <summary>
/// Represents a semantic meta block used in card body (avatar + title + description).
/// </summary>
public sealed class CardMeta : UserControl
{
    public static readonly MewProperty<Element?> AvatarProperty =
        MewProperty<Element?>.Register<CardMeta>(nameof(Avatar), null,
            MewPropertyOptions.AffectsLayout,
            static (self, _, _) => self.Build());

    public static readonly MewProperty<Element?> TitleProperty =
        MewProperty<Element?>.Register<CardMeta>(nameof(Title), null,
            MewPropertyOptions.AffectsLayout,
            static (self, _, _) => self.Build());

    public static readonly MewProperty<Element?> DescriptionProperty =
        MewProperty<Element?>.Register<CardMeta>(nameof(Description), null,
            MewPropertyOptions.AffectsLayout,
            static (self, _, _) => self.Build());

    /// <summary>
    /// Gets or sets the avatar element.
    /// </summary>
    public Element? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    /// <summary>
    /// Gets or sets the meta title element.
    /// </summary>
    public Element? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the meta description element.
    /// </summary>
    public Element? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public CardMeta()
    {
        Build();
    }

    protected override Element OnBuild()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
        };

        if (Avatar != null)
        {
            row.Add(Avatar);
        }

        var content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
        };

        if (Title != null)
        {
            content.Add(Title);
        }

        if (Description != null)
        {
            content.Add(Description);
        }

        if (content.Count > 0)
        {
            row.Add(content);
        }

        return row;
    }
}
