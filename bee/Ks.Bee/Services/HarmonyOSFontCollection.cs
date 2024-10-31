using System;
using Avalonia.Media.Fonts;

namespace Ks.Bee.Services;

public sealed class HarmonyOSFontCollection : EmbeddedFontCollection
{
    public HarmonyOSFontCollection() : base(
        new Uri("fonts:HarmonyOS Sans", UriKind.Absolute),
        new Uri("avares://Ks.Bee/Assets/Fonts", UriKind.Absolute))
    {
    }
}
