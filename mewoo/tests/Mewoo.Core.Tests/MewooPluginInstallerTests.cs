using System.IO.Compression;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooPluginInstallerTests
{
    [TestMethod]
    public void InstallValidPackagePromotesFilesIntoPluginRoot()
    {
        using var directory = TestInstallDirectory.Create();
        var packagePath = directory.WritePackage(
            "xkyss.installPlugin",
            "0.1.0",
            ("InstallPlugin.dll", "plugin bytes"),
            ("PrivateDependency.dll", "dependency bytes"));

        var result = new MewooPluginInstaller().Install(packagePath, directory.PluginRoot);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("xkyss.installPlugin", result.PluginId);
        Assert.AreEqual(Path.Combine(directory.PluginRoot, "xkyss.installPlugin"), result.InstalledPath);
        Assert.AreEqual(MewooPluginTrustDiagnostics.LocalCodeTrustWarning, result.TrustWarning);
        Assert.IsTrue(File.Exists(Path.Combine(result.InstalledPath!, MewooPluginPackageFormat.ManifestEntryName)));
        Assert.IsTrue(File.Exists(Path.Combine(result.InstalledPath!, "InstallPlugin.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(result.InstalledPath!, "PrivateDependency.dll")));
    }

    [TestMethod]
    public void InstallInvalidPackageReturnsFailureWithoutWritingPluginRoot()
    {
        using var directory = TestInstallDirectory.Create();
        var packagePath = directory.WritePackageWithoutManifest(("InstallPlugin.dll", "plugin bytes"));

        var result = new MewooPluginInstaller().Install(packagePath, directory.PluginRoot);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Issue);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Package, result.Issue.Category);
        Assert.IsFalse(Directory.Exists(directory.PluginRoot));
    }

    [TestMethod]
    public void InstallRefusesExpectedPluginIdMismatchWithoutWritingPluginRoot()
    {
        using var directory = TestInstallDirectory.Create();
        var packagePath = directory.WritePackage(
            "xkyss.installPlugin",
            "0.1.0",
            ("InstallPlugin.dll", "plugin bytes"));

        var result = new MewooPluginInstaller().Install(
            packagePath,
            directory.PluginRoot,
            expectedPluginId: "xkyss.otherPlugin");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("xkyss.installPlugin", result.PluginId);
        Assert.IsNotNull(result.Issue);
        StringAssert.Contains(result.Issue.ShortMessage, "does not match expected id");
        Assert.IsFalse(Directory.Exists(directory.PluginRoot));
    }

    [TestMethod]
    public void InstallExistingPluginReplacesInstalledDirectory()
    {
        using var directory = TestInstallDirectory.Create();
        var firstPackage = directory.WritePackage(
            "xkyss.installPlugin",
            "0.1.0",
            ("InstallPlugin.dll", "old plugin bytes"),
            ("OldDependency.dll", "old dependency bytes"));
        var secondPackage = directory.WritePackage(
            "xkyss.installPlugin",
            "0.2.0",
            [
                ("InstallPlugin.dll", "new plugin bytes"),
                ("NewDependency.dll", "new dependency bytes"),
            ],
            packageFileName: "second");
        var installer = new MewooPluginInstaller();

        var first = installer.Install(firstPackage, directory.PluginRoot);
        var second = installer.Install(secondPackage, directory.PluginRoot);

        Assert.IsTrue(first.Success);
        Assert.IsTrue(second.Success);
        var installedPath = Path.Combine(directory.PluginRoot, "xkyss.installPlugin");
        Assert.IsTrue(File.Exists(Path.Combine(installedPath, "NewDependency.dll")));
        Assert.IsFalse(File.Exists(Path.Combine(installedPath, "OldDependency.dll")));
        StringAssert.Contains(
            File.ReadAllText(Path.Combine(installedPath, MewooPluginPackageFormat.ManifestEntryName)),
            "\"version\": \"0.2.0\"");
    }

    [TestMethod]
    public void InstallRejectsPackageEntriesThatEscapeStagingDirectory()
    {
        using var directory = TestInstallDirectory.Create();
        var packagePath = directory.WritePackage(
            "xkyss.installPlugin",
            "0.1.0",
            ("InstallPlugin.dll", "plugin bytes"),
            ("../escape.txt", "bad"));

        var result = new MewooPluginInstaller().Install(packagePath, directory.PluginRoot);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Issue);
        StringAssert.Contains(result.Issue.ShortMessage, "escapes the install directory");
        Assert.IsFalse(File.Exists(Path.Combine(directory.Root, "escape.txt")));
    }

    private sealed class TestInstallDirectory : IDisposable
    {
        private TestInstallDirectory(string root)
        {
            Root = root;
            PackageRoot = Path.Combine(root, "packages");
            PluginRoot = Path.Combine(root, "plugins");
            Directory.CreateDirectory(PackageRoot);
        }

        public string Root { get; }

        public string PackageRoot { get; }

        public string PluginRoot { get; }

        public static TestInstallDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestInstallDirectory(root);
        }

        public string WritePackage(
            string pluginId,
            string version,
            params (string EntryName, string Contents)[] entries)
        {
            return WritePackage(pluginId, version, entries, "test");
        }

        public string WritePackage(
            string pluginId,
            string version,
            (string EntryName, string Contents)[] entries,
            string packageFileName)
        {
            var manifestJson = $$"""
                {
                  "id": "{{pluginId}}",
                  "displayName": "Install Plugin",
                  "version": "{{version}}",
                  "assembly": "InstallPlugin.dll",
                  "entryPoint": "Xkyss.InstallPlugin.Plugin"
                }
                """;
            var packagePath = Path.Combine(PackageRoot, packageFileName + MewooPluginPackageFormat.Extension);
            using var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create);
            WriteEntry(archive, MewooPluginPackageFormat.ManifestEntryName, manifestJson);
            foreach (var entry in entries)
            {
                WriteEntry(archive, entry.EntryName, entry.Contents);
            }

            return packagePath;
        }

        public string WritePackageWithoutManifest(params (string EntryName, string Contents)[] entries)
        {
            var packagePath = Path.Combine(PackageRoot, "test" + MewooPluginPackageFormat.Extension);
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

        private static void WriteEntry(ZipArchive archive, string entryName, string contents)
        {
            var entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(contents);
        }
    }
}
