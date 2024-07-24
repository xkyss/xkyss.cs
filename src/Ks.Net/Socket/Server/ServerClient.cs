using System.Buffers;
using System.IO.Pipelines;
using Ks.Core.System.Buffers;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Middlewares;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Server;

internal sealed class ServerClient(IServiceProvider sp, ILogger<ServerClient> logger, ISocketEncoder encoder, ISocketDecoder decoder)
    : ISocketClient
{
    private readonly NetDelegate<SocketContext<ServerClient>> net = new NetBuilder<SocketContext<ServerClient>>(sp)
        .Use<FallbackMiddleware<ServerClient>>()
        .Build();
    
    internal ConnectionContext Context { get; set; }

    internal PipeWriter Writer => Context.Transport.Output;

    public Task WriteAsync<T>(T message) where T : Message
    {
        var bodyBytes = encoder.Encode(message.GetType(), message);
        
        var response = new SocketResponse();
        response.MessageLength = bodyBytes.Length;
        var headerBytes = encoder.Encode(response);
        
        Writer.WriteBigEndian(headerBytes.Length);
        Writer.Write(headerBytes);
        Writer.Write(bodyBytes);
        return Writer.FlushAsync().AsTask();
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

            if (TryReadRequest(result, out var request, out var consumed))
            {
                var response = new SocketResponse();
                var socketConnect = new SocketContext<ServerClient>(this, request, response, context.Features);
                await net.Invoke(socketConnect);
                input.AdvanceTo(consumed);
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

    private bool TryReadRequest(ReadResult result, out SocketRequest request, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        request = SocketRequest.Empty;
        consumed = result.Buffer.Start;
        
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
        
        reader.TryReadExact(headLen, out var headerBytes);
        request = decoder.Decode<SocketRequest>(headerBytes);
        
        // 检测长度
        if (request.MessageLength <= 0)
        {
            return false;
        }
        
        // 读取Message
        // TODO: 当前写死HeartBeat
        reader.TryReadExact(request.MessageLength, out var bodyBytes);
        request.Message = decoder.Decode<HeartBeat>(bodyBytes);
        
        consumed = reader.Position;
        return true;
    }
    
}
