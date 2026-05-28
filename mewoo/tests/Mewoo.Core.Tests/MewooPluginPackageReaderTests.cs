using System.IO.Compression;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooPluginPackageReaderTests
{
    [TestMethod]
    public void ReadValidPackageReturnsManifestMetadataAndAssemblyEntry()
    {
        using var directory = TestPackageDirectory.Create();
        var packagePath = directory.WritePackage(
            """
            {
              "id": "xkyss.packagePlugin",
              "displayName": "Package Plugin",
              "version": "0.1.0",
              "assembly": "PackagePlugin.dll",
              "entryPoint": "Xkyss.PackagePlugin.Plugin",
              "metadata": {
                "publisher": "xkyss",
                "publisherDisplayName": "xkyss labs"
              }
            }
            """,
            ("PackagePlugin.dll", "plugin bytes"),
            ("Dependency.dll", "dependency bytes"));

        var result = new MewooPluginPackageReader().Read(packagePath);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Package);
        Assert.IsNull(result.Issue);
        Assert.AreEqual("xkyss.packagePlugin", result.Package.PackageId);
        Assert.AreEqual("Package Plugin", result.Package.DisplayName);
        Assert.AreEqual("0.1.0", result.Package.Version);
        Assert.AreEqual("xkyss", result.Package.Publisher);
        Assert.AreEqual("xkyss labs", result.Package.PublisherDisplayName);
        Assert.AreEqual(MewooPluginPackageFormat.ManifestEntryName, result.Package.ManifestEntryName);
        Assert.AreEqual("PackagePlugin.dll", result.Package.AssemblyEntryName);
    }

    [TestMethod]
    public void ReadRejectsWrongExtension()
    {
        using var directory = TestPackageDirectory.Create();
        var packagePath = directory.WritePackage(
            """
            {
              "id": "xkyss.packagePlugin",
              "displayName": "Package Plugin",
              "version": "0.1.0",
              "assembly": "PackagePlugin.dll",
              "entryPoint": "Xkyss.PackagePlugin.Plugin"
            }
            """,
            ("PackagePlugin.dll", "plugin bytes"),
            extension: ".zip");

        var result = new MewooPluginPackageReader().Read(packagePath);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Issue);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Package, result.Issue.Category);
        StringAssert.Contains(result.Issue.ShortMessage, MewooPluginPackageFormat.Extension);
    }

    [TestMethod]
    public void ReadRejectsPackageWithoutRootManifest()
    {
        using var directory = TestPackageDirectory.Create();
        var packagePath = directory.WritePackageWithoutManifest(("PackagePlugin.dll", "plugin bytes"));

        var result = new MewooPluginPackageReader().Read(packagePath);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Issue);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Package, result.Issue.Category);
        StringAssert.Contains(result.Issue.ShortMessage, MewooPluginPackageFormat.ManifestEntryName);
    }

    [TestMethod]
    public void ReadRejectsPackageWithoutManifestAssembly()
    {
        using var directory = TestPackageDirectory.Create();
        var packagePath = directory.WritePackage(
            """
            {
              "id": "xkyss.packagePlugin",
              "displayName": "Package Plugin",
              "version": "0.1.0",
              "assembly": "PackagePlugin.dll",
              "entryPoint": "Xkyss.PackagePlugin.Plugin"
            }
            """);

        var result = new MewooPluginPackageReader().Read(packagePath);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Issue);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Assembly, result.Issue.Category);
        Assert.AreEqual("PackagePlugin.dll", result.Issue.AssemblyPath);
    }

    [TestMethod]
    public void ReadRejectsInvalidManifestWithoutExtractingPackage()
    {
        using var directory = TestPackageDirectory.Create();
        var packagePath = directory.WritePackage(
            """
            {
              "id": "bad-id",
              "displayName": "Package Plugin",
              "version": "0.1.0",
              "assembly": "PackagePlugin.dll",
              "entryPoint": "Xkyss.PackagePlugin.Plugin"
            }
            """,
            ("PackagePlugin.dll", "plugin bytes"));

        var result = new MewooPluginPackageReader().Read(packagePath);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Issue);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Package, result.Issue.Category);
        StringAssert.Contains(result.Issue.ShortMessage, "invalid plugin id");
        Assert.IsFalse(Directory.Exists(Path.Combine(directory.Root, "xkyss.packagePlugin")));
    }

    private sealed class TestPackageDirectory : IDisposable
    {
        private TestPackageDirectory(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TestPackageDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestPackageDirectory(root);
        }

        public string WritePackage(
            string manifestJson,
            params (string EntryName, string Contents)[] entries)
        {
            return WritePackage(manifestJson, entries, MewooPluginPackageFormat.Extension);
        }

        public string WritePackage(
            string manifestJson,
            (string EntryName, string Contents) entry,
            string extension)
        {
            return WritePackage(manifestJson, [entry], extension);
        }

        public string WritePackageWithoutManifest(params (string EntryName, string Contents)[] entries)
        {
            var packagePath = Path.Combine(Root, "test" + MewooPluginPackageFormat.Extension);
            using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
            foreach (var entry in entries)
            {
                WriteEntry(archive, entry.EntryName, entry.Contents);
            }

            return packagePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private string WritePackage(
            string manifestJson,
            IReadOnlyList<(string EntryName, string Contents)> entries,
            string extension)
        {
            var packagePath = Path.Combine(Root, "test" + extension);
            using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
            WriteEntry(archive, MewooPluginPackageFormat.ManifestEntryName, manifestJson);
            foreach (var entry in entries)
            {
                WriteEntry(archive, entry.EntryName, entry.Contents);
            }

            return packagePath;
        }

        private static void WriteEntry(ZipArchive archive, string entryName, string contents)
        {
            var entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(contents);
        }
    }
}

