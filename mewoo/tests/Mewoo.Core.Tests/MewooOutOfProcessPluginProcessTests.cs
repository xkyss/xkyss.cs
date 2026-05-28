using Mewoo.Core.Plugins;

namespace Mewoo.Core.Tests;

[TestClass]
public sealed class MewooOutOfProcessPluginProcessTests
{
    [TestMethod]
    public async Task ActivateAndShutdownProbeHost()
    {
        var hostAssembly = Path.Combine(
            AppContext.BaseDirectory,
            "Mewoo.TestPlugins.OutOfProcessProbeHost.dll");
        Assert.IsTrue(File.Exists(hostAssembly), hostAssembly);
        await using var process = new MewooOutOfProcessPluginProcess();

        var activated = await process.ActivateAsync(new MewooOutOfProcessPluginProcessOptions(
            "xkyss.outOfProcessProbe",
            "dotnet",
            Quote(hostAssembly)));

        Assert.IsTrue(activated.Success, activated.Message);
        Assert.IsTrue(process.IsRunning);

        var shutdown = await process.ShutdownAsync();

        Assert.IsTrue(shutdown);
        Assert.IsFalse(process.IsRunning);
    }

    private static string Quote(string value) => $"\"{value}\"";
}
