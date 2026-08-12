using Mew.Workbench;
using Xunit;
using WorkbenchType = Mew.Workbench.Workbench;

namespace Mew.Launcher.Tests;

/// <summary>
/// Launcher 模块化(票据 05):LauncherApp 实现 IMewToolModule,Configure 经 ToolModuleContext
/// 贡献五区与浮层搜索源;临时引导(宿主未建)下仍可组装运行。
/// 与 WorkbenchConfigurationTests 同集合串行:两者都操作真实 %APPDATA%\Mew 布局文件,
/// 且 MewDock 在 Build 后持有文件句柄,并行会互相踩文件。
/// </summary>
[Collection("IsolatedUserFiles")]
public class LauncherModuleTests
{
    /// <summary>记录注册的搜索源的浮层契约替身(测试环境不建 MewUI 窗口)。</summary>
    private sealed class RecordingOverlay : IOverlayService
    {
        public List<ISearchSource> Sources { get; } = [];

        public void AddSearchSource(ISearchSource source) => Sources.Add(source);
    }

    [Fact]
    public void Configure_贡献五区_注册搜索源_Build通过()
    {
        using var _ = IsolateUserFiles();

        var workbench = new WorkbenchType();
        var settings = new SettingsService(Path.Combine(Path.GetTempPath(), "mew-module-tests", Guid.NewGuid().ToString("N") + ".json"));
        var overlay = new RecordingOverlay();
        var context = new ToolModuleContext(workbench, IntPtr.Zero, new ScaffoldHotkeyService(), settings, overlay, workbench.ThemeContext);

        var module = new LauncherApp();
        module.Configure(context);

        // 五区配对/ID 唯一校验通过;设置文档可运行时打开(注册生效)
        workbench.Build();
        workbench.OpenDocument("settings-document");

        Assert.Equal("launcher", module.Id);
        Assert.Equal("启动项", module.DisplayName);
        var source = Assert.Single(overlay.Sources);
        Assert.Equal("launcher", source.Id);
    }

    /// <summary>临时移走用户真实数据/布局文件,保证用例不污染用户数据且任意本机可复现。</summary>
    private static IDisposable IsolateUserFiles()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mew");
        var names = new[] { "launcher.json", "layout.json", "presentation.json" };
        var saved = names.ToDictionary(name => name, name => File.Exists(Path.Combine(appData, name))
            ? File.ReadAllBytes(Path.Combine(appData, name))
            : null);
        foreach (var name in names)
        {
            File.Delete(Path.Combine(appData, name));
        }

        return new Disposable(() =>
        {
            foreach (var (name, bytes) in saved)
            {
                var path = Path.Combine(appData, name);
                if (bytes is not null)
                {
                    Directory.CreateDirectory(appData);
                    File.WriteAllBytes(path, bytes);
                }
                else
                {
                    File.Delete(path);
                }
            }
        });
    }

    private sealed class Disposable(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
