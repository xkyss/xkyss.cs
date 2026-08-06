using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mew.Workbench;
using Xunit;
using WorkbenchType = Mew.Workbench.Workbench;

namespace Mew.Launcher.Tests;

public class WorkbenchConfigurationTests
{
    [Fact]
    public void Build_停靠组件跨区域重名_拒绝配置()
    {
        var workbench = new WorkbenchType()
            .ActivityBar(bar => bar.Item("launch", "启动", GlyphKind.Hamburger))
            .SideBar(side => side.View("launch", "启动", new StackPanel()))
            .EditorArea(editor => editor.Document("launch", "启动项", new StackPanel()));

        var exception = Assert.Throws<InvalidOperationException>(workbench.Build);

        Assert.Equal("侧边栏、编辑器区与底部面板的停靠组件 ID 必须全局唯一。", exception.Message);
    }
}
