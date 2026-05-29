using System.IO.Compression;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooPluginInstallFlowTests
{
    [TestMethod]
    public void PreviewValidPackageShowsIdentityPublisherTrustAndPermissions()
    {
        using var directory = TestInstallFlowDirectory.Create();
        var packagePath = directory.WritePackage(
            """
            {
              "id": "xkyss.previewPlugin",
              "displayName": "Preview Plugin",
              "version": "0.1.0",
              "assembly": "PreviewPlugin.dll",
              "entryPoint": "Xkyss.PreviewPlugin.Plugin",
              "trust": {
                "trustedLocalCode": true
              },
              "permissions": [
                {
                  "kind": "network",
                  "reason": "Fetches data."
                }
              ],
              "metadata": {
                "publisher": "xkyss",
                "publisherDisplayName": "xkyss labs"
              }
            }
            """,
            ("PreviewPlugin.dll", "plugin bytes"));

        var preview = new MewooPluginInstallPreviewer().Preview(packagePath);

        Assert.IsTrue(preview.Success);
        Assert.AreEqual("xkyss.previewPlugin", preview.PluginId);
        Assert.AreEqual("Preview Plugin", preview.DisplayName);
        Assert.AreEqual("0.1.0", preview.Version);
        Assert.AreEqual("xkyss", preview.Publisher);
        Assert.AreEqual("xkyss labs", preview.PublisherDisplayName);
        Assert.AreEqual("Trusted local code declared", preview.TrustLabel);
        CollectionAssert.AreEqual(new[] { "Network" }, preview.PermissionLabels.ToArray());
        Assert.AreEqual(MewooPluginTrustDiagnostics.LocalCodeTrustWarning, preview.TrustWarning);
    }

    [TestMethod]
    public void PreviewInvalidPackageShowsFailureAndConservativePermissionMessage()
    {
        using var directory = TestInstallFlowDirectory.Create();
        var packagePath = directory.WritePackageWithoutManifest(("PreviewPlugin.dll", "plugin bytes"));

        var preview = new MewooPluginInstallPreviewer().Preview(packagePath);

        Assert.IsFalse(preview.Success);
        Assert.IsNotNull(preview.Issue);
        Assert.AreEqual("Unknown local code", preview.TrustLabel);
        StringAssert.Contains(preview.PermissionSummary, "unavailable");
    }

    [TestMethod]
    public void CreateResultSummaryMapsInstallSuccessAndFailure()
    {
        var success = MewooPluginInstallFlowDisplay.CreateResultSummary(
            MewooPluginInstallResult.Succeeded("xkyss.previewPlugin", "plugins/xkyss.previewPlugin"));
        var failure = MewooPluginInstallFlowDisplay.CreateResultSummary(
            MewooPluginInstallResult.Failed(new MewooRuntimePluginIssue(
                MewooRuntimePluginIssueCategory.Package,
                "Package is invalid.",
                "bad.mewoo-plugin")));

        Assert.IsTrue(success.Success);
        Assert.AreEqual("Installed plugin 'xkyss.previewPlugin'.", success.Message);
        Assert.IsFalse(failure.Success);
        Assert.AreEqual("Package is invalid.", failure.Message);
    }

    private sealed class TestInstallFlowDirectory : IDisposable
    {
        private TestInstallFlowDirectory(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TestInstallFlowDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestInstallFlowDirectory(root);
        }

        public string WritePackage(
            string manifestJson,
            params (string EntryName, string Contents)[] entries)
        {
            var packagePath = Path.Combine(Root, "test" + MewooPluginPackageFormat.Extension);
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
            var packagePath = Path.Combine(Root, "bad" + MewooPluginPackageFormat.Extension);
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
