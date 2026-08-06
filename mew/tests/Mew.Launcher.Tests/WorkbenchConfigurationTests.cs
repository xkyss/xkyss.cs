using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.MewDock;
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

    /// <summary>
    /// 回归:设置文档 tab 关闭后重新打开时内容必须重新挂接回停靠树(否则 tab 显示空白)。
    /// 此前 OpenDocument 把共享内容实例(原始 StackPanel)作为显式内容传入,而 ContentFactory
    /// 解析出的是新的 Border 包装;MewDock 的 SyncContent 分离旧 Border 后,因共享元素的
    /// Parent 仍指向旧包装而无法重新挂接,重新打开的 tab 内容为空。
    /// </summary>
    [Fact]
    public void OpenDocument_关闭后重新打开_内容仍挂接在停靠树()
    {
        // Workbench 的布局存储读取 %APPDATA%\Mew\layout.json;测试期间临时移走用户真实布局,
        // 结束后原样恢复,保证用例在任意本机状态下可复现且不污染用户数据。
        var layoutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mew", "layout.json");
        var presentationPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mew", "presentation.json");
        var savedLayout = File.Exists(layoutPath) ? File.ReadAllBytes(layoutPath) : null;
        var savedPresentation = File.Exists(presentationPath) ? File.ReadAllBytes(presentationPath) : null;
        File.Delete(layoutPath);
        File.Delete(presentationPath);

        try
        {
            var settingsContent = new StackPanel();
            var workbench = new WorkbenchType()
                .ActivityBar(bar =>
                {
                    bar.Item("launch", "启动", GlyphKind.Hamburger);
                    bar.Item("settings", "设置", GlyphKind.Hamburger);
                })
                .SideBar(side => side
                    .View("launch", "启动", new StackPanel())
                    .View("settings", "设置", new StackPanel()))
                .EditorArea(editor => editor
                    .Document("items", "启动项", new StackPanel())
                    .Document("settings-document", "设置", settingsContent));

            var shell = workbench.Build();
            var dock = FindByType(shell, typeof(DockingManager)) as DockingManager
                ?? throw new InvalidOperationException("未找到 DockingManager。");

            // 打开设置文档 → 内容挂接在停靠树中
            workbench.OpenDocument("settings-document");
            Assert.True(IsInLiveTree(shell, settingsContent), "打开后设置内容应挂接在停靠树中。");

            // 关闭设置 tab → 文档 pane 消失,内容离开停靠树
            var pane = dock.DocumentPanes.FirstOrDefault(p => p.Component == "settings-document");
            Assert.NotNull(pane);
            pane!.Close();
            Assert.DoesNotContain(dock.DocumentPanes, p => p.Component == "settings-document");

            // 重新打开设置文档 → 内容再次挂接(修复前此处内容被孤立,tab 空白)
            workbench.OpenDocument("settings-document");
            Assert.True(IsInLiveTree(shell, settingsContent), "重新打开后设置内容应重新挂接在停靠树中。");

            // 激活其他文档(相当于点击侧边栏「全部」)后切回设置 tab,内容仍保持挂接
            workbench.OpenDocument("items");
            workbench.OpenDocument("settings-document");
            Assert.True(IsInLiveTree(shell, settingsContent), "切换文档后设置内容仍应保持挂接。");
        }
        finally
        {
            if (savedLayout is not null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(layoutPath)!);
                File.WriteAllBytes(layoutPath, savedLayout);
            }
            else
            {
                File.Delete(layoutPath);
            }

            if (savedPresentation is not null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(presentationPath)!);
                File.WriteAllBytes(presentationPath, savedPresentation);
            }
            else
            {
                File.Delete(presentationPath);
            }
        }
    }

    /// <summary>从指定根元素向下查找目标类型的第一个元素。</summary>
    private static UIElement? FindByType(UIElement root, Type type)
    {
        if (type.IsInstanceOfType(root))
        {
            return root;
        }

        if (root is IVisualTreeHost host)
        {
            UIElement? found = null;
            host.VisitChildren(child =>
            {
                found = FindByType((UIElement)child, type);
                return found is null;
            });
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>目标元素是否仍挂接在根元素的可见树中(沿 IVisualTreeHost 深度优先可达,即实际可见)。</summary>
    private static bool IsInLiveTree(UIElement root, UIElement target)
    {
        if (ReferenceEquals(root, target))
        {
            return true;
        }

        if (root is IVisualTreeHost host)
        {
            var found = false;
            host.VisitChildren(child =>
            {
                if (IsInLiveTree((UIElement)child, target))
                {
                    found = true;
                    return false;
                }

                return true;
            });
            return found;
        }

        return false;
    }
}
