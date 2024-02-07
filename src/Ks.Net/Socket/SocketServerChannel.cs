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

        public SocketServerChannel(ConnectionContext context, ILogger<SocketServerChannel> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task RunAsync()
        {
            try
            {
                // _ = TrySendAsync();

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

        private bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out Message o)
        {
            o = new Message();
            return true;
        }
    }
}