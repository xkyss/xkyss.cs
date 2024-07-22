using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Ks.Core.System.Buffers;
using Ks.Net.Kestrel;
using Ks.Net.Socket.Middlewares;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket;

internal sealed class ConnectedClient(IServiceProvider sp, ILogger<ConnectedClient> logger)
{
    private readonly NetDelegate<SocketServerContext> net = new NetBuilder<SocketServerContext>(sp)
        .Use(c => context =>
        {
            logger.LogWarning(context.Request.Message);
            context.Client.WriteLineAsync(context.Request.Message);
            return Task.CompletedTask;
        })
        .Build();
    
    internal ConnectionContext Context { get; set; }

    internal PipeWriter Writer => Context.Transport.Output;

    public ValueTask<FlushResult> WriteAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Write(text, encoding);
        return FlushAsync();
    }

    public ConnectedClient Write(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Writer.Write(text, encoding ?? Encoding.UTF8);
        return this;
    }
    
    public ValueTask<FlushResult> WriteLineAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        WriteLine(text, encoding);
        return FlushAsync();
    }

    public ConnectedClient WriteLine(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Writer.Write(text, encoding ?? Encoding.UTF8);
        Writer.WriteCRLF();
        return this;
    }

    public ValueTask<FlushResult> FlushAsync()
    {
        return Writer.FlushAsync();
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
                var socketConnect = new SocketServerContext(this, request, response, context.Features);
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
