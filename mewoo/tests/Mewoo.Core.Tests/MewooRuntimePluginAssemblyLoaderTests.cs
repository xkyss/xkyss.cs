using System.Reflection;
using Mewoo.Abstractions.Plugins;
using Mewoo.Core.Logging;
using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooRuntimePluginAssemblyLoaderTests
{
    [TestMethod]
    public void TryLoadLoadsManifestAssembly()
    {
        using var directory = TestRuntimePluginDirectory.Create();
        var sourceAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var assemblyPath = directory.CopyAssembly(sourceAssemblyPath);
        var descriptor = directory.CreateDescriptor(Path.GetFileName(assemblyPath));

        var loaded = new MewooRuntimePluginAssemblyLoader().TryLoad(descriptor, out var issue);

        Assert.IsNotNull(loaded);
        Assert.IsNull(issue);
        Assert.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, loaded.Assembly.GetName().Name);
        loaded.LoadContext.Unload();
    }

    [TestMethod]
    public void TryLoadLogsMissingAssemblyAndReturnsNull()
    {
        using var directory = TestRuntimePluginDirectory.Create();
        var descriptor = directory.CreateDescriptor("MissingPlugin.dll");
        var logger = new InMemoryMewooLogger();

        var loaded = new MewooRuntimePluginAssemblyLoader(logger).TryLoad(descriptor, out var issue);

        Assert.IsNull(loaded);
        Assert.IsNotNull(issue);
        Assert.AreEqual(MewooRuntimePluginIssueCategory.Assembly, issue.Category);
        Assert.AreEqual(descriptor.ManifestPath, issue.ManifestPath);
        Assert.AreEqual(descriptor.AssemblyPath, issue.AssemblyPath);
        Assert.IsTrue(logger.Entries.Any(entry =>
            entry.Message.Contains("Failed to load runtime plugin assembly", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void LoadContextResolvesAbstractionsFromDefaultContext()
    {
        using var directory = TestRuntimePluginDirectory.Create();
        var sourceAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var assemblyPath = directory.CopyAssembly(sourceAssemblyPath);
        var context = new MewooRuntimePluginLoadContext(assemblyPath);
        var abstractionName = typeof(IMewooPlugin).Assembly.GetName();

        var resolved = context.ResolveAssembly(abstractionName);

        Assert.AreSame(typeof(IMewooPlugin).Assembly, resolved);
        context.Unload();
    }

    private sealed class TestRuntimePluginDirectory : IDisposable
    {
        private TestRuntimePluginDirectory(string root)
        {
            Root = root;
            PluginDirectory = Path.Combine(root, "runtimePlugin");
            Directory.CreateDirectory(PluginDirectory);
        }

        public string Root { get; }

        public string PluginDirectory { get; }

        public static TestRuntimePluginDirectory Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "Mewoo.Core.Tests", Guid.NewGuid().ToString("N"));
            return new TestRuntimePluginDirectory(root);
        }

        public string CopyAssembly(string sourceAssemblyPath)
        {
            var target = Path.Combine(PluginDirectory, Path.GetFileName(sourceAssemblyPath));
            File.Copy(sourceAssemblyPath, target);
            return target;
        }

        public MewooRuntimePluginDescriptor CreateDescriptor(string assemblyFileName)
        {
            var manifest = new MewooPluginManifest
            {
                Id = "xkyss.runtimePlugin",
                DisplayName = "Runtime Plugin",
                Version = "0.1.0",
                Assembly = assemblyFileName,
                EntryPoint = "RuntimePlugin.Plugin",
            };
            var manifestPath = Path.Combine(PluginDirectory, MewooRuntimePluginCatalog.ManifestFileName);
            var assemblyPath = Path.Combine(PluginDirectory, assemblyFileName);

            return new MewooRuntimePluginDescriptor(
                manifest,
                manifestPath,
                PluginDirectory,
                assemblyPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                TryDeleteRoot();
            }
        }

        private void TryDeleteRoot()
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    Directory.Delete(Root, recursive: true);
                    return;
                }
                catch (IOException)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (UnauthorizedAccessException)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }
}
