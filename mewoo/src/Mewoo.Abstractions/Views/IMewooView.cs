namespace Mewoo.Abstractions.Views;

public interface IMewooView
{
    string Id { get; }

    object NativeView { get; }
}

public interface IMewooViewContext
{
    string PluginId { get; }

    IServiceProvider Services { get; }

    IWorkbenchService Workbench { get; }
}

public sealed class MewooView(string id, object nativeView) : IMewooView
{
    public string Id { get; } = id;

    public object NativeView { get; } = nativeView;
}

