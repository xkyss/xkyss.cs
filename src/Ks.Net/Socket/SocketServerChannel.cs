using System.Buffers;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket
{
    public class SocketServerChannel : NetChannel
    {
        public event Func<Message, Task> OnMessageHandler;

        private readonly ConnectionContext _context;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _sendSemaphore = new(0);
        private readonly MemoryStream _sendStream = new();
        
        public SocketServerChannel(ConnectionContext context, ILogger<SocketServerChannel> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task RunAsync()
        {
            try
            {
                _ = TrySendAsync();

                var cancelToken = CloseTokenSource.Token;
                while (!cancelToken.IsCancellationRequested)
                {
                    var result = await _context.Transport.Input.ReadAsync(cancelToken);
                    var buffer = result.Buffer;
                    if (buffer.Length > 0)
                    {
                        while (TryParseMessage(ref buffer, out var msg))
                        {
                            await OnMessageHandler.Invoke(msg);
                        }
                        _context.Transport.Input.AdvanceTo(buffer.Start, buffer.End);
                    }
                    else if (result.IsCanceled || result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return;
            }
            
            _logger.LogInformation("RunAsync end.");
        }
        
        async Task TrySendAsync()
        {
            //pipewriter线程不安全，这里统一发送写刷新数据
            try
            {
                var token = CloseTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    await _sendSemaphore.WaitAsync();
                    lock (_sendStream)
                    {
                        var len = _sendStream.Length;
                        if (len > 0)
                        {
                            _context.Transport.Output.Write(_sendStream.GetBuffer().AsSpan<byte>()[..(int)len]);
                            _sendStream.SetLength(0);
                            _sendStream.Position = 0;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    await _context.Transport.Output.FlushAsync(token);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            };
        }

        private bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out Message o)
        {
            o = new Message();
            return false;
        }
    }
}