using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Mewoo.Abstractions;

namespace Mewoo.Controls.Sidebar;

public sealed class SidebarNavigation
{
    private readonly IWorkbenchService _workbench;
    private readonly SidebarNavigationOptions _options;
    private readonly List<SidebarNavigationNode> _nodes = [];
    private readonly HashSet<string> _uncontrolledExpandedIds = new(StringComparer.Ordinal);
    private readonly Dictionary<TreeViewNode, SidebarNavigationNode> _nodesByTreeNode = [];
    private TreeView? _host;
    private bool _isSyncingSelection;

    private SidebarNavigation(IWorkbenchService workbench, SidebarNavigationOptions? options)
    {
        _workbench = workbench;
        _options = options ?? new SidebarNavigationOptions();
        if (_options.InitialExpandedNodeIds is not null)
        {
            _uncontrolledExpandedIds.UnionWith(_options.InitialExpandedNodeIds);
        }
    }

    public static SidebarNavigation Create(IWorkbenchService workbench, SidebarNavigationOptions? options = null) =>
        new(workbench, options);

    public SidebarNavigation Node(string id, string title, Action<SidebarNavigationNodeBuilder> configure)
    {
        _nodes.Add(SidebarNavigationNodeBuilder.Build(id, title, configure));
        return this;
    }

    public SidebarNavigation MainView(string id, string title, string mainViewId)
    {
        _nodes.Add(SidebarNavigationNode.MainView(id, title, mainViewId));
        return this;
    }

    public SidebarNavigation Command(string id, string title, string commandId)
    {
        _nodes.Add(SidebarNavigationNode.Command(id, title, commandId));
        return this;
    }

    public SidebarNavigation Action(string id, string title, Action action) =>
        Action(id, title, _ =>
        {
            action();
            return ValueTask.CompletedTask;
        });

    public SidebarNavigation Action(string id, string title, Func<Task> action) =>
        Action(id, title, async _ => await action());

    public SidebarNavigation Action(string id, string title, Func<CancellationToken, ValueTask> action)
    {
        _nodes.Add(SidebarNavigationNode.CustomAction(id, title, action));
        return this;
    }

    public TreeView Build()
    {
        _host = new TreeView
        {
            BorderThickness = 0,
            ExpandTrigger = TreeViewExpandTrigger.ClickNode,
            Indent = 12,
            ItemHeight = 30,
            ItemPadding = new Thickness(8, 4, 8, 4),
        }.Margin(8, 1);
        _host.SelectedNodeChanged += OnSelectedNodeChanged;
        _workbench.StateChanged += OnWorkbenchStateChanged;
        Render();
        return _host;
    }

    private void OnSelectedNodeChanged(TreeViewNode? selectedNode)
    {
        if (_isSyncingSelection || selectedNode is null)
        {
            return;
        }

        if (!_nodesByTreeNode.TryGetValue(selectedNode, out var node) || !IsNodeEnabled(node))
        {
            return;
        }

        SidebarActionRunner.ForMenuItem(ExecuteNodeAsync(node), _options.Controls).Invoke();
    }

    private void OnWorkbenchStateChanged(object? sender, WorkbenchViewStateChangedEventArgs e)
    {
        Render();
    }

    private void Render()
    {
        if (_host is null)
        {
            return;
        }

        if (_options.ExpandedNodeIds is null && _options.AutoExpandSelectedAncestors)
        {
            ExpandSelectedAncestors();
        }

        _nodesByTreeNode.Clear();
        TreeViewNode? selectedNode = null;
        var treeNodes = _nodes
            .Select(node => CreateTreeNode(node, depth: 0, ref selectedNode))
            .ToArray();

        _host.Tag = treeNodes;
        _host.ItemsSource(treeNodes);
        foreach (var treeNode in treeNodes)
        {
            ApplyExpansionState(treeNode);
        }

        _isSyncingSelection = true;
        try
        {
            _host.SelectedNode = selectedNode;
        }
        finally
        {
            _isSyncingSelection = false;
        }
    }

    private void ApplyExpansionState(TreeViewNode treeNode)
    {
        if (treeNode.Tag is not SidebarNavigationRowState state || !treeNode.HasChildren)
        {
            return;
        }

        if (state.IsExpanded)
        {
            _host?.Expand(treeNode);
        }
        else
        {
            _host?.Collapse(treeNode);
        }

        foreach (var child in treeNode.Children)
        {
            ApplyExpansionState(child);
        }
    }

    private TreeViewNode CreateTreeNode(SidebarNavigationNode node, int depth, ref TreeViewNode? selectedNode)
    {
        var isExpanded = IsExpanded(node.Id);
        var isSelected = IsSelected(node);
        var containsActive = !isSelected && ContainsActive(node);
        var children = new List<TreeViewNode>(node.Children.Count);
        foreach (var child in node.Children)
        {
            children.Add(CreateTreeNode(child, depth + 1, ref selectedNode));
        }
        var rowState = new SidebarNavigationRowState(
            node.Id,
            depth,
            isExpanded,
            isSelected,
            containsActive);
        var treeNode = new TreeViewNode(node.Title, children, rowState);
        _nodesByTreeNode[treeNode] = node;
        if (isSelected)
        {
            selectedNode = treeNode;
        }

        return treeNode;
    }

    private Func<CancellationToken, ValueTask> ExecuteNodeAsync(SidebarNavigationNode node)
    {
        return async cancellationToken =>
        {
            if (node.Children.Count > 0 && node.Kind == SidebarNavigationNodeKind.Group)
            {
                ToggleExpanded(node.Id);
                return;
            }

            switch (node.Kind)
            {
                case SidebarNavigationNodeKind.MainView:
                    await _workbench.OpenMainViewAsync(node.MainViewId!, cancellationToken);
                    break;
                case SidebarNavigationNodeKind.Command:
                    if (_options.ExecuteCommandAsync is null)
                    {
                        throw new InvalidOperationException(
                            $"Sidebar navigation command '{node.CommandId}' cannot run without a command executor.");
                    }

                    await _options.ExecuteCommandAsync(node.CommandId!, cancellationToken);
                    break;
                case SidebarNavigationNodeKind.CustomAction:
                    await node.Action!(cancellationToken);
                    break;
                case SidebarNavigationNodeKind.Group:
                    ToggleExpanded(node.Id);
                    break;
            }
        };
    }

    private bool IsNodeEnabled(SidebarNavigationNode node)
    {
        return node.IsEnabled
            && (node.Kind != SidebarNavigationNodeKind.Command || _options.ExecuteCommandAsync is not null);
    }

    private bool IsSelected(SidebarNavigationNode node)
    {
        return node.Kind == SidebarNavigationNodeKind.MainView
            && string.Equals(node.MainViewId, _workbench.Current.ActiveMainViewId, StringComparison.Ordinal);
    }

    private bool ContainsActive(SidebarNavigationNode node)
    {
        return node.Children.Any(child => IsSelected(child) || ContainsActive(child));
    }

    private bool IsExpanded(string nodeId)
    {
        return _options.ExpandedNodeIds is not null
            ? _options.ExpandedNodeIds.Contains(nodeId)
            : _uncontrolledExpandedIds.Contains(nodeId);
    }

    private void ToggleExpanded(string nodeId)
    {
        if (_options.ExpandedNodeIds is not null)
        {
            var expandedIds = _options.ExpandedNodeIds.ToHashSet(StringComparer.Ordinal);
            if (!expandedIds.Add(nodeId))
            {
                expandedIds.Remove(nodeId);
            }

            _options.OnExpandedNodeIdsChanged?.Invoke(expandedIds);
            return;
        }

        if (!_uncontrolledExpandedIds.Add(nodeId))
        {
            _uncontrolledExpandedIds.Remove(nodeId);
        }

        Render();
    }

    private void ExpandSelectedAncestors()
    {
        foreach (var node in _nodes)
        {
            ExpandSelectedAncestors(node);
        }
    }

    private bool ExpandSelectedAncestors(SidebarNavigationNode node)
    {
        if (IsSelected(node))
        {
            return true;
        }

        var containsActive = false;
        foreach (var child in node.Children)
        {
            containsActive = ExpandSelectedAncestors(child) || containsActive;
        }

        if (containsActive)
        {
            _uncontrolledExpandedIds.Add(node.Id);
        }

        return containsActive;
    }
}

public sealed record SidebarNavigationOptions
{
    public SidebarControlOptions? Controls { get; init; }

    public IReadOnlySet<string>? ExpandedNodeIds { get; init; }

    public IReadOnlySet<string>? InitialExpandedNodeIds { get; init; }

    public Action<IReadOnlySet<string>>? OnExpandedNodeIdsChanged { get; init; }

    public bool AutoExpandSelectedAncestors { get; init; } = true;

    public Func<string, CancellationToken, ValueTask>? ExecuteCommandAsync { get; init; }
}

public sealed record SidebarNavigationRowState(
    string NodeId,
    int Depth,
    bool IsExpanded,
    bool IsSelected,
    bool ContainsActive);

public sealed class SidebarNavigationNodeBuilder
{
    private readonly List<SidebarNavigationNode> _children = [];

    private SidebarNavigationNodeBuilder(string id, string title)
    {
        Id = id;
        Title = title;
    }

    public string Id { get; }

    public string Title { get; }

    public SidebarNavigationNodeBuilder Node(
        string id,
        string title,
        Action<SidebarNavigationNodeBuilder> configure)
    {
        _children.Add(Build(id, title, configure));
        return this;
    }

    public SidebarNavigationNodeBuilder MainView(string id, string title, string mainViewId)
    {
        _children.Add(SidebarNavigationNode.MainView(id, title, mainViewId));
        return this;
    }

    public SidebarNavigationNodeBuilder Command(string id, string title, string commandId)
    {
        _children.Add(SidebarNavigationNode.Command(id, title, commandId));
        return this;
    }

    public SidebarNavigationNodeBuilder Action(string id, string title, Action action) =>
        Action(id, title, _ =>
        {
            action();
            return ValueTask.CompletedTask;
        });

    public SidebarNavigationNodeBuilder Action(string id, string title, Func<Task> action) =>
        Action(id, title, async _ => await action());

    public SidebarNavigationNodeBuilder Action(
        string id,
        string title,
        Func<CancellationToken, ValueTask> action)
    {
        _children.Add(SidebarNavigationNode.CustomAction(id, title, action));
        return this;
    }

    internal static SidebarNavigationNode Build(
        string id,
        string title,
        Action<SidebarNavigationNodeBuilder> configure)
    {
        var builder = new SidebarNavigationNodeBuilder(id, title);
        configure(builder);
        return SidebarNavigationNode.Group(id, title, builder._children);
    }
}

internal enum SidebarNavigationNodeKind
{
    Group,
    MainView,
    Command,
    CustomAction,
}

internal sealed record SidebarNavigationNode(
    string Id,
    string Title,
    SidebarNavigationNodeKind Kind,
    IReadOnlyList<SidebarNavigationNode> Children,
    string? MainViewId = null,
    string? CommandId = null,
    Func<CancellationToken, ValueTask>? Action = null,
    string? TrailingText = null,
    bool IsEnabled = true)
{
    public static SidebarNavigationNode Group(
        string id,
        string title,
        IReadOnlyList<SidebarNavigationNode> children) =>
        new(id, title, SidebarNavigationNodeKind.Group, children);

    public static SidebarNavigationNode MainView(string id, string title, string mainViewId) =>
        new(id, title, SidebarNavigationNodeKind.MainView, [], MainViewId: mainViewId);

    public static SidebarNavigationNode Command(string id, string title, string commandId) =>
        new(id, title, SidebarNavigationNodeKind.Command, [], CommandId: commandId);

    public static SidebarNavigationNode CustomAction(
        string id,
        string title,
        Func<CancellationToken, ValueTask> action) =>
        new(id, title, SidebarNavigationNodeKind.CustomAction, [], Action: action);
}
