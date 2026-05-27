using Mewoo.Abstractions.Plugins;

namespace Mewoo.Core.Plugins;

public sealed record MewooRuntimePluginDescriptor(
    MewooPluginManifest Manifest,
    string ManifestPath,
    string PluginDirectory,
    string AssemblyPath);

