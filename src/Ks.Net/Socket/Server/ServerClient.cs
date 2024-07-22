using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ks.Core.System.Buffers;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Middlewares;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket.Server;

internal sealed class ServerClient(IServiceProvider sp, ILogger<ServerClient> logger)
    : ISocketClient<ServerClient>
{
    private readonly NetDelegate<SocketContext<ServerClient>> net = new NetBuilder<SocketContext<ServerClient>>(sp)
        .Use(c => context =>
        {
            logger.LogWarning(context.Request.Message);
            context.Client.WriteLineAsync(context.Request.Message);
            return Task.CompletedTask;
        })
        .Use<FallbackMiddleware<ServerClient>>()
        .Build();
    
    internal ConnectionContext Context { get; set; }

    internal PipeWriter Writer => Context.Transport.Output;

    public Task WriteAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Write(text, encoding);
        return FlushAsync();
    }

    public ServerClient Write(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Writer.Write(text, encoding ?? Encoding.UTF8);
        return this;
    }
    
    public Task WriteLineAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        WriteLine(text, encoding);
        return FlushAsync();
    }

    public ServerClient WriteLine(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Writer.Write(text, encoding ?? Encoding.UTF8);
        Writer.WriteCRLF();
        return this;
    }

    public Task FlushAsync()
    {
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

    private static bool TryReadRequest(ReadResult result, out SocketRequest request, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (reader.TryReadTo(out ReadOnlySpan<byte> span, Constants.CRLF))
        {
            request = new SocketRequest { Message = Encoding.UTF8.GetString(span) };
            consumed = reader.Position;
            return true;
        }
        else
        {
            request = SocketRequest.Empty;
            consumed = result.Buffer.Start;
            return false;
        }
    }
    
}
