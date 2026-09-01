using System.IO.Pipes;
using System.Text.Json;

namespace Mew.Workbench.Ipc;

/// <summary>
/// 宿主侧 IPC 服务：管理注册的搜索源客户端，校验协议版本与能力越权，提供跨源搜索聚合。
/// 对应 spec 03：JSON-RPC over NamedPipe，宿主为 server，管道名含用户隔离。
/// 本实现提供内存态用于测试，NamedPipe 监听在 StartAsync 中可选启用。
/// </summary>
public sealed class IpcServer
{
    private readonly List<IpcClientHandle> _clients = [];
    private readonly object _lock = new();

    public IReadOnlyList<IpcClientHandle> Clients
    {
        get { lock (_lock) return _clients.ToList(); }
    }

    /// <summary>注册客户端；协议版本不匹配或能力越权则返回错误。</summary>
    public RegisterAckMessage TryRegister(RegisterMessage msg)
    {
        if (msg.ProtocolVersion != IpcProtocol.CurrentVersion)
            return new RegisterAckMessage(false, $"协议版本不匹配（期望 {IpcProtocol.CurrentVersion}，实际 {msg.ProtocolVersion}）");

        lock (_lock)
        {
            if (_clients.Any(c => string.Equals(c.Id, msg.Id, StringComparison.OrdinalIgnoreCase)))
                return new RegisterAckMessage(false, $"id 重复：{msg.Id}");
        }

        // 能力越权在后续 Search/Settings/Hotkey 调用时拒绝，本票仅记录注册信息
        return new RegisterAckMessage(true, null);
    }

    /// <summary>测试或本地直连用：注册一个内存搜索源客户端。</summary>
    public bool RegisterInMemoryClient(string id, string displayName, int protocolVersion, PluginCapabilitiesDto? capabilities, ISearchSource? source, out string? error)
    {
        var ack = TryRegister(new RegisterMessage(id, displayName, capabilities, protocolVersion));
        if (!ack.Ok) { error = ack.Error; return false; }
        if (source != null && capabilities?.Search == null)
        {
            error = "未声明 search 能力，拒绝注册搜索源";
            return false;
        }
        lock (_lock) _clients.Add(new IpcClientHandle(id, displayName, protocolVersion, capabilities, source));
        error = null;
        return true;
    }

    public void Unregister(string id)
    {
        lock (_lock) _clients.RemoveAll(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>跨源搜索聚合：向所有已注册客户端扇出 search，收集结果后扁平混排。</summary>
    public IReadOnlyList<OverlayResultEntry> Search(string query, int globalCap = OverlaySearchAggregator.DefaultGlobalCap)
    {
        List<ISearchSource> sources;
        lock (_lock) sources = _clients.Select(c => c.Source).Where(s => s != null).Cast<ISearchSource>().ToList();
        // 复用既有聚合策略：每源上限 = 全局上限/源数
        return OverlaySearchAggregator.Aggregate(sources, query, globalCap);
    }

    /// <summary>跨源搜索（DTO 视角，供真实 IPC 管道序列化后使用）。</summary>
    public IReadOnlyList<SearchResultDto> SearchDto(string query, int globalCap = OverlaySearchAggregator.DefaultGlobalCap)
    {
        var entries = Search(query, globalCap);
        return entries.Select(e => new SearchResultDto(
            Guid.NewGuid().ToString("N"),
            e.Result.Title,
            e.Result.Subtitle,
            e.Source.Id,
            e.Source.DisplayName
        )).ToList();
    }

    // ---- NamedPipe 监听（宿主为 server） ----
    private CancellationTokenSource? _cts;

    public void Start()
    {
        if (_cts != null) return;
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        try { _cts?.Cancel(); } catch { }
        _cts = null;
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var server = new NamedPipeServerStream(IpcProtocol.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            try
            {
                await server.WaitForConnectionAsync(ct);
                _ = Task.Run(() => HandlePipeClientAsync(server, ct), ct);
            }
            catch (OperationCanceledException) { server.Dispose(); break; }
            catch { server.Dispose(); await Task.Delay(200, ct); }
        }
    }

    private async Task HandlePipeClientAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        using (pipe)
        {
            var reader = new StreamReader(pipe);
            var writer = new StreamWriter(pipe) { AutoFlush = true };
            string? line;
            IpcClientHandle? handle = null;
            try
            {
                line = await reader.ReadLineAsync(ct);
                if (line == null) return;
                var reg = JsonSerializer.Deserialize(line, IpcJsonContext.Default.RegisterMessage);
                if (reg == null) return;
                var ack = TryRegister(reg);
                if (!ack.Ok)
                {
                    await writer.WriteLineAsync(JsonSerializer.Serialize(ack, IpcJsonContext.Default.RegisterAckMessage));
                    return;
                }
                // 延迟创建 handle：pipe 客户端的搜索源通过远端调用，无本地 ISearchSource，标记为 pipe 客户端
                lock (_lock) _clients.Add(handle = new IpcClientHandle(reg.Id, reg.DisplayName, reg.ProtocolVersion, reg.Capabilities, new PipeSearchProxy(pipe, writer, reader, reg.Id, reg.DisplayName)));
                await writer.WriteLineAsync(JsonSerializer.Serialize(new RegisterAckMessage(true, null), IpcJsonContext.Default.RegisterAckMessage));
                // 保持连接直到断开
                while (!ct.IsCancellationRequested && pipe.IsConnected)
                {
                    await Task.Delay(500, ct);
                }
            }
            catch { }
            finally
            {
                if (handle != null) Unregister(handle.Id);
            }
        }
    }

    private sealed class PipeSearchProxy : ISearchSource
    {
        private readonly NamedPipeServerStream _pipe;
        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;
        public PipeSearchProxy(NamedPipeServerStream pipe, StreamWriter writer, StreamReader reader, string id, string displayName) { _pipe = pipe; _writer = writer; _reader = reader; Id = id; DisplayName = displayName; }
        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<SearchResult> Search(string query, int maxResults)
        {
            try
            {
                var reqId = Guid.NewGuid().ToString("N");
                var req = new SearchRequestMessage(reqId, query, maxResults);
                var json = JsonSerializer.Serialize(req, IpcJsonContext.Default.SearchRequestMessage);
                lock (_writer) { _writer.WriteLine(json); _writer.Flush(); }
                // 简化：同步等待响应（带超时）
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                while (!cts.IsCancellationRequested)
                {
                    if (_reader.ReadLine() is string line && !string.IsNullOrWhiteSpace(line))
                    {
                        var resp = JsonSerializer.Deserialize(line, IpcJsonContext.Default.SearchResponseMessage);
                        if (resp != null && resp.RequestId == reqId)
                            return resp.Results.Select(dto => new SearchResult(dto.Title, dto.Subtitle, null, () => { })).ToList();
                    }
                }
            }
            catch { }
            return [];
        }
    }
}

public sealed class IpcClientHandle
{
    public IpcClientHandle(string id, string displayName, int protocolVersion, PluginCapabilitiesDto? capabilities, ISearchSource? source)
    {
        Id = id;
        DisplayName = displayName;
        ProtocolVersion = protocolVersion;
        Capabilities = capabilities;
        Source = source;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public int ProtocolVersion { get; }
    public PluginCapabilitiesDto? Capabilities { get; }
    public ISearchSource? Source { get; }
}
