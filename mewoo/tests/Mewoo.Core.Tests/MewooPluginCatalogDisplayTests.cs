using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooPluginCatalogDisplayTests
{
    [TestMethod]
    public void CreateEntriesMapsRuntimeStatesToCatalogStates()
    {
        var entries = MewooPluginCatalogDisplay.CreateEntries(
            [
                Status("installed", MewooRuntimePluginState.Discovered),
                Status("disabled", MewooRuntimePluginState.Disabled),
                Status("incompatible", MewooRuntimePluginState.Incompatible),
                Status("failed", MewooRuntimePluginState.Failed),
                Status("loaded", MewooRuntimePluginState.Loaded),
                Status("registered", MewooRuntimePluginState.Registered),
            ],
            [
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Manifest,
                    "Manifest is invalid.",
                    "plugins/bad/mewoo.plugin.json"),
            ]);

        CollectionAssert.AreEqual(
            new[]
            {
                MewooPluginCatalogEntryState.Installed,
                MewooPluginCatalogEntryState.Disabled,
                MewooPluginCatalogEntryState.Incompatible,
                MewooPluginCatalogEntryState.Failed,
                MewooPluginCatalogEntryState.Loaded,
                MewooPluginCatalogEntryState.Loaded,
                MewooPluginCatalogEntryState.Discovered,
            },
            entries.Select(entry => entry.State).ToArray());
    }

    [TestMethod]
    public void CreateEntriesIncludesPublisherMetadataWhenManifestDeclaresIt()
    {
        var entries = MewooPluginCatalogDisplay.CreateEntries(
            [
                Status(
                    "published",
                    MewooRuntimePluginState.Registered,
                    new Dictionary<string, string>
                    {
                        [MewooPluginPackageFormat.PublisherMetadataKey] = "xkyss",
                        [MewooPluginPackageFormat.PublisherDisplayNameMetadataKey] = "xkyss labs",
                    }),
            ],
            []);

        Assert.AreEqual("0.1.0", entries[0].Version);
        Assert.AreEqual("xkyss", entries[0].Publisher);
        Assert.AreEqual("xkyss labs", entries[0].PublisherDisplayName);
    }

    [TestMethod]
    public void CreateOperationMapsSuccessAndFailureMessages()
    {
        var timestamp = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        var success = MewooPluginCatalogDisplay.CreateOperation(
            "Install",
            MewooPluginOperationResult.Succeeded("plugin.one", "plugins/plugin.one"),
            timestamp);
        var failure = MewooPluginCatalogDisplay.CreateOperation(
            "Update",
            MewooPluginOperationResult.Failed(
                "plugin.two",
                new MewooRuntimePluginIssue(
                    MewooRuntimePluginIssueCategory.Package,
                    "Package is invalid.",
                    "plugin.two.mewoo-plugin")),
            timestamp);

        Assert.IsTrue(success.Success);
        Assert.AreEqual("Install succeeded.", success.Message);
        Assert.IsFalse(failure.Success);
        Assert.AreEqual("Package is invalid.", failure.Message);
    }

    private static MewooRuntimePluginStatus Status(
        string id,
        MewooRuntimePluginState state,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        var manifest = new MewooPluginManifest
        {
            Id = id,
            DisplayName = id,
            Version = "0.1.0",
            Assembly = $"{id}.dll",
            EntryPoint = $"{id}.Plugin",
            Metadata = metadata ?? new Dictionary<string, string>(),
        };

        return new MewooRuntimePluginStatus(
            new MewooRuntimePluginDescriptor(
                manifest,
                $"plugins/{id}/{MewooRuntimePluginCatalog.ManifestFileName}",
                $"plugins/{id}",
                $"plugins/{id}/{id}.dll"),
            state);
    }
}
