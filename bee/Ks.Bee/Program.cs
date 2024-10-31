﻿using Avalonia;
using System;
using Avalonia.Media;
using Ks.Bee.Services;

namespace Ks.Bee;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureFonts(fontManager =>
            {
                fontManager.AddFontCollection(new HarmonyOSFontCollection());
            })
            .With(new FontManagerOptions()
            {
                DefaultFamilyName = "fonts:HarmonyOS Sans#HarmonyOS Sans SC"
            })
            .LogToTrace();
}
