using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ks.Core.System.Buffers;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Middlewares;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Server;

internal sealed class ServerClient(IServiceProvider sp, ILogger<ServerClient> logger)
    : ISocketClient
{
    private readonly NetDelegate<SocketContext<ServerClient>> net = new NetBuilder<SocketContext<ServerClient>>(sp)
        .Use(c => context =>
        {
            // logger.LogWarning(context.Request.Message);
            // context.Client.WriteLineAsync(context.Request.Message);
            return Task.CompletedTask;
        })
        .Use<FallbackMiddleware<ServerClient>>()
        .Build();
    
    internal ConnectionContext Context { get; set; }

    internal PipeWriter Writer => Context.Transport.Output;

    public void Write<T>(T message) where T : Message
    {
        var bodyBytes = MessagePackSerializer.Serialize(message);
        
        var response = new SocketResponse();
        response.MessageLength = bodyBytes.Length;
        var headerBytes = MessagePackSerializer.Serialize(response);
        
        Writer.WriteBigEndian(headerBytes.Length);
        Writer.Write(headerBytes);
        Writer.Write(bodyBytes);
        Writer.FlushAsync();
    }

    public bool IsClose()
    {
        return Context.ConnectionClosed.IsCancellationRequested;
    }

    public Task StartAsync()
    {
        logger.LogInformation($"[{Context.ConnectionId}]加入连接.");
        return HandleRequestsAsync(Context);
    }

    public Task StopAsync()
    {
        logger.LogInformation($"[{Context.ConnectionId}]断开连接.");
        return Context.DisposeAsync().AsTask();
    }

    private async Task HandleRequestsAsync(ConnectionContext context)
    {
        var input = context.Transport.Input;
        while (context.ConnectionClosed.IsCancellationRequested == false)
        {
            var result = await input.ReadAsync();
            if (result.IsCanceled)
            {
                break;
            }

            if (TryReadRequest(result, out var request))
            {
                var response = new SocketResponse();
                var socketConnect = new SocketContext<ServerClient>(this, request, response, context.Features);
                await net.Invoke(socketConnect);
            }
            else
            {
                input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
            }

            if (result.IsCompleted)
            {
                break;
            }
        }
    }

    private static bool TryReadRequest(ReadResult result, out SocketRequest request)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        
        request = SocketRequest.Empty;
        
        // 消息头部长度
        if (!reader.TryReadBigEndian(out int headLen))
        {
            return false;
        }
        
        // 检测长度
        if (headLen <= 0)
        {
            return false;
        }
        
        var headerBytes = result.Buffer.Slice(reader.Position, headLen);
        request = MessagePackSerializer.Deserialize<SocketRequest>(headerBytes);
        
        // 检测长度
        if (request.MessageLength <= 0)
        {
            return false;
        }

        // TODO: 当前写死HeartBeat
        reader.Advance(headLen);
        var bodyBytes = result.Buffer.Slice(reader.Position);
        request.Message = MessagePackSerializer.Deserialize<HeartBeat>(bodyBytes);
        return true;
    }
    
}
