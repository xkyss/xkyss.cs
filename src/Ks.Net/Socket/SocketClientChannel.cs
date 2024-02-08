using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket
{
    public class SocketClientChannel : NetChannel
    {
        private readonly Pipe _receivePipe = new();
        private readonly TcpClient _socket = new (AddressFamily.InterNetwork)
        {
            NoDelay = true
        };
        private readonly ILogger _logger;

        public SocketClientChannel(ILogger<SocketClientChannel> logger)
        {
            _logger = logger;
        }

        public Task ConnectAsync(string ip, int port)
        {
            return _socket.ConnectAsync(ip, port);
        }

        public override Task RunAsync()
        {
            _ = ReceiveOnceAsync();
            _ = ReceiveAsync();
            return Task.CompletedTask;
        }

        private async Task ReceiveAsync()
        {
            try
            {
                var cancelToken = CloseTokenSource.Token;
                while (!cancelToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ReceiveAsync in while.");
                    var result = await _receivePipe.Reader.ReadAsync(cancelToken);
                    var buffer = result.Buffer;
                    if (buffer.Length > 0)
                    {
                        while (TryParseMessage(ref buffer))
                        {
                            
                        };
                        
                        _receivePipe.Reader.AdvanceTo(buffer.Start, buffer.End);
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
            
            _logger.LogInformation("ReceiveAsync 结束.");
        }
        
        private async Task ReceiveOnceAsync()
        {
            try
            {
                var readBuffer = new byte[2048];
                var dataPipeWriter = _receivePipe.Writer;
                var cancelToken = CloseTokenSource.Token;
                while (!cancelToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ReceiveOnceAsync in while.");
                    var length = await _socket.GetStream().ReadAsync(readBuffer, cancelToken);
                    if (length > 0)
                    {
                        dataPipeWriter.Write(readBuffer.AsSpan()[..length]);
                        var flushTask = dataPipeWriter.FlushAsync();
                        if (!flushTask.IsCompleted)
                        {
                            await flushTask.ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return;
            };
            
            _logger.LogInformation("ReceiveOnceAsync 结束.");
        }

        protected virtual bool TryParseMessage(ref ReadOnlySequence<byte> input)
        {
            return false;
        }

        public override Task Write(Message message)
        {
            var bytes = "Hello"u8.ToArray();
            lock(_socket)
            {
                _socket.GetStream().Write(bytes);
            }

            return Task.CompletedTask;
        }
    }
}