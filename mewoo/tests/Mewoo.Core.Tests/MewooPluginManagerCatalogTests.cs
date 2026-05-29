using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooPluginManagerCatalogTests
{
    [TestMethod]
    public void CreateEntriesMapsRuntimeStatesToPluginManagerStates()
    {
        using var directory = TestPluginManagerCatalogDirectory.Create();
        var entries = new MewooPluginManagerCatalog().CreateEntries(
            directory.PluginRoot,
            [
                Status("enabled", MewooRuntimePluginState.Discovered),
                Status("disabled", MewooRuntimePluginState.Disabled),
                Status("loaded", MewooRuntimePluginState.Registered),
                Status("failed", MewooRuntimePluginState.Failed),
                Status("incompatible", MewooRuntimePluginState.Incompatible),
                Status(
                    "missingAssembly",
                    MewooRuntimePluginState.Failed,
                    issueCategory: MewooRuntimePluginIssueCategory.Assembly),
            ],
            []);

        CollectionAssert.AreEqual(
            new[]
            {
                MewooPluginManagerCatalogState.Disabled,
                MewooPluginManagerCatalogState.Enabled,
                MewooPluginManagerCatalogState.Failed,
                MewooPluginManagerCatalogState.Incompatible,
                MewooPluginManagerCatalogState.Loaded,
                MewooPluginManagerCatalogState.Broken,
            },
            entries.Select(entry => entry.State).ToArray());
    }

    [TestMethod]
    public void CreateEntriesIncludesIdentityPublisherTrustAndPermissions()
    {
        using var directory = TestPluginManagerCatalogDirectory.Create();

        var entries = new MewooPluginManagerCatalog().CreateEntries(
            directory.PluginRoot,
            [
                Status(
                    "published",
                    MewooRuntimePluginState.Registered,
                    metadata: new Dictionary<string, string>
                    {
                        [MewooPluginPackageFormat.PublisherMetadataKey] = "xkyss",
                        [MewooPluginPackageFormat.PublisherDisplayNameMetadataKey] = "xkyss labs",
                    },
                    trust: new MewooPluginTrustDeclaration { TrustedLocalCode = true },
                    permissions:
                    [
                        new MewooPluginPermissionDeclaration { Kind = MewooPluginPermissionKinds.Filesystem },
                        new MewooPluginPermissionDeclaration { Kind = MewooPluginPermissionKinds.ProcessLaunch },
                    ]),
            ],
            []);

        Assert.AreEqual("published", entries[0].PluginId);
        Assert.AreEqual("0.1.0", entries[0].Version);
        Assert.AreEqual("xkyss", entries[0].Publisher);
        Assert.AreEqual("xkyss labs", entries[0].PublisherDisplayName);
        Assert.AreEqual("Trusted local code declared", entries[0].TrustLabel);
        CollectionAssert.AreEqual(
            new[] { "Filesystem", "Process launch" },
            entries[0].PermissionLabels.ToArray());
    }

    [TestMethod]
    public void CreateEntriesIncludesBrokenInstalledDirectories()
    {
        using var directory = TestPluginManagerCatalogDirectory.Create();
        directory.CreateBrokenDirectory("missingManifest");
        directory.CreateInvalidManifestDirectory("invalidManifest");
        directory.CreateInvalidIdDirectory("invalidId");
        directory.CreateMissingAssemblyDirectory("missingAssembly");

        var entries = new MewooPluginManagerCatalog().CreateEntries(
            directory.PluginRoot,
            [],
            []);

        CollectionAssert.AreEqual(
            new[] { "invalidId", "invalidManifest", "missingAssembly", "missingManifest" },
            entries.Select(entry => entry.DisplayName).ToArray());
        Assert.IsTrue(entries.All(entry => entry.State == MewooPluginManagerCatalogState.Broken));
        Assert.IsTrue(entries.All(entry => !string.IsNullOrWhiteSpace(entry.Message)));
        Assert.IsTrue(entries.Any(entry => entry.Message.Contains("invalid plugin id", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(entries.Any(entry => entry.Message.Contains("manifest", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(entries.Any(entry => entry.Message.Contains("assembly", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void CreateEntriesAppliesSearchAndFilter()
    {
        using var directory = TestPluginManagerCatalogDirectory.Create();
        var entries = new MewooPluginManagerCatalog().CreateEntries(
            directory.PluginRoot,
            [
                Status("alpha", MewooRuntimePluginState.Registered),
                Status(
                    "beta",
                    MewooRuntimePluginState.Disabled,
                    metadata: new Dictionary<string, string>
                    {
                        [MewooPluginPackageFormat.PublisherMetadataKey] = "needle",
                    }),
                Status("gamma", MewooRuntimePluginState.Failed),
            ],
            [],
            new MewooPluginManagerCatalogQuery("needle", MewooPluginManagerCatalogFilter.Disabled));

        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("beta", entries[0].PluginId);
        Assert.AreEqual(MewooPluginManagerCatalogState.Disabled, entries[0].State);
    }

    private static MewooRuntimePluginStatus Status(
        string id,
        MewooRuntimePluginState state,
        IReadOnlyDictionary<string, string>? metadata = null,
        MewooPluginTrustDeclaration? trust = null,
        IReadOnlyList<MewooPluginPermissionDeclaration>? permissions = null,
        MewooRuntimePluginIssueCategory issueCategory = MewooRuntimePluginIssueCategory.None)
    {
        var manifest = new MewooPluginManifest
        {
            Id = id,
            DisplayName = id,
            Version = "0.1.0",
            Assembly = $"{id}.dll",
            EntryPoint = $"{id}.Plugin",
            Trust = trust,
            Permissions = permissions ?? [],
            Metadata = metadata ?? new Dictionary<string, string>(),
        };

        return new MewooRuntimePluginStatus(
            new MewooRuntimePluginDescriptor(
                manifest,
                $"plugins/{id}/{MewooRuntimePluginCatalog.ManifestFileName}",
                $"plugins/{id}",
                $"plugins/{id}/{id}.dll"),
            state,
            issueCategory == MewooRuntimePluginIssueCategory.None ? null : "Runtime issue.",
            issueCategory);
    }

    private sealed class TestPluginManagerCatalogDirectory : IDisposable
    {
        private TestPluginManagerCatalogDirectory(string root)
        {
            Root = root;
            PluginRoot = Path.Combine(root, "plugins");
            Directory.CreateDirectory(PluginRoot);
        }

        public string Root { get; }

        public string PluginRoot { get; }

        public static TestPluginManagerCatalogDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestPluginManagerCatalogDirectory(root);
        }

        public void CreateBrokenDirectory(string name)
        {
            Directory.CreateDirectory(Path.Combine(PluginRoot, name));
        }

        public void CreateInvalidManifestDirectory(string name)
        {
            var directory = Path.Combine(PluginRoot, name);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, MewooRuntimePluginCatalog.ManifestFileName), "{ bad json");
        }

        public void CreateInvalidIdDirectory(string name)
        {
            var directory = Path.Combine(PluginRoot, name);
            Directory.CreateDirectory(directory);
            File.WriteAllText(
                Path.Combine(directory, MewooRuntimePluginCatalog.ManifestFileName),
                """
                {
                  "id": "bad id",
                  "displayName": "Invalid Id",
                  "version": "0.1.0",
                  "assembly": "InvalidId.dll",
                  "entryPoint": "Xkyss.Plugin"
                }
                """);
        }

        public void CreateMissingAssemblyDirectory(string name)
        {
            var directory = Path.Combine(PluginRoot, name);
            Directory.CreateDirectory(directory);
            File.WriteAllText(
                Path.Combine(directory, MewooRuntimePluginCatalog.ManifestFileName),
                $$"""
                {
                  "id": "xkyss.{{name}}",
                  "displayName": "{{name}}",
                  "version": "0.1.0",
                  "assembly": "{{name}}.dll",
                  "entryPoint": "Xkyss.Plugin"
                }
                """);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
