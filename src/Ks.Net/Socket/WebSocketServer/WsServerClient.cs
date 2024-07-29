using System.Buffers;
using System.Net.WebSockets;
using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.WebSocketServer;

internal sealed class WsServerClient(
    ILogger<WsServerClient> logger
    , ISocketEncoder encoder
    , ISocketDecoder decoder
    , ISocketTypeMapper typeMapper
    , NetDelegate<SocketContext> net
) : ISocketClient
{
    internal ConnectionContext Context { get; set; }
    
    internal WebSocket WebSocket { get; set; }

    public Task WriteAsync<T>(T message)
    {
        if (message == null)
        {
            logger.LogWarning("消息为空.");
            return Task.CompletedTask;
        }
        
        var type = message.GetType();
        if (!typeMapper.TryGet(type, out var id))
        {
            logger.LogWarning($"消息{type}未注册.");
            return Task.CompletedTask;
        }
        
        var bodyBytes = encoder.Encode(type, message);
        var headerBytes = encoder.Encode(new SocketRequest
        {
            MessageTypeId = id,
            MessageLength = bodyBytes.Length
        });

        // 写入响应头长度（4字节，大端序）
        var headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            // 确保使用大端序
            Array.Reverse(headerLengthBytes);
        }

        // 合并bytes
        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);
        binaryWriter.Write(headerLengthBytes);
        binaryWriter.Write(headerBytes);
        binaryWriter.Write(bodyBytes);
        var data = memoryStream.ToArray();
        
        return WebSocket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
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
        return WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socketclose", CancellationToken.None);
    }

    private async Task HandleRequestsAsync(ConnectionContext context)
    {
        using var stream = new MemoryStream();
        var buffer = new ArraySegment<byte>(new byte[2048]);
        
        while (context.ConnectionClosed.IsCancellationRequested == false)
        {
            WebSocketReceiveResult result;
            stream.SetLength(0);
            stream.Seek(0, SeekOrigin.Begin);
            do
            {
                result = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                stream.Write(buffer.Array, buffer.Offset, result.Count);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            stream.Seek(0, SeekOrigin.Begin);

            var response = new SocketResponse();
            TryReadRequest(stream, out var request);
            var socketConnect = new SocketContext(this, request, response, context.Features);
            await net.Invoke(socketConnect);
        }
    }

    private bool TryReadRequest(MemoryStream stream, out SocketRequest request)
    {
        // 使用 MemoryStream 的内存直接读取,减少一次拷贝
        stream.Position = 0;
        var sequence = new ReadOnlySequence<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        var reader = new SequenceReader<byte>(sequence);
        
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

        if (!reader.TryReadExact(headLen, out var headerBytes))
        {
            return false;
        }

        try
        {
            request = decoder.Decode<SocketRequest>(headerBytes);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "解析[SocketRequest]失败");
            return false;
        }
        
        // 检测长度
        if (request.MessageLength <= 0)
        {
            return false;
        }
        
        // 读取Message
        if (!reader.TryReadExact(request.MessageLength, out var bodyBytes))
        {
            return false;
        }

        if (!typeMapper.TryGet(request.MessageTypeId, out var type))
        {
            return false;
        }
        
        request.Message = decoder.Decode(type, bodyBytes);
        return true;
    }
}
