﻿using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ks.Core.System.Buffers;
using Ks.Net.Kestrel;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Server;

internal sealed class ServerClient(
    ILogger<ServerClient> logger
    , ISocketEncoder encoder
    , ISocketDecoder decoder
    , ISocketTypeMapper typeMapper
    , NetDelegate<SocketContext> net
) : ISocketClient
{
    internal ConnectionContext Context { get; set; }

    internal PipeWriter Writer => Context.Transport.Output;
    
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

        // 响应头长度（4字节，大端序）
        var headerLengthBytes = BitConverter.GetBytes(headerBytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            // 确保使用大端序
            Array.Reverse(headerLengthBytes);
        }
        
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
                input.AdvanceTo(consumed);

                var response = new SocketResponse();
                var socketConnect = new SocketContext(this, request, response, context.Features);
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
        
        consumed = reader.Position;
        return true;
    }
    
}
