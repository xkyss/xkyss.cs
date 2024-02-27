using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
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
            _ = ReceiveNetAsync();
            _ = ReceivePipAsync();
            return Task.CompletedTask;
        }

        private async Task ReceivePipAsync()
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
        
        private async Task ReceiveNetAsync()
        {
            try
            {
                var readBuffer = new byte[2048];
                var dataPipeWriter = _receivePipe.Writer;
                var cancelToken = CloseTokenSource.Token;
                var stream = _socket.GetStream();
                while (!cancelToken.IsCancellationRequested)
                {
                    var end = false;
                    _logger.LogInformation("ReceiveOnceAsync in while.");
                    do
                    {
                        var length = await stream.ReadAsync(readBuffer, cancelToken);
                        if (length <= 0)
                        {
                            end = true;
                            break;
                        }
                        
                        dataPipeWriter.Write(readBuffer.AsSpan()[..length]);

                    } while (stream.DataAvailable);

                    if (!end)
                    {
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
            var s = Encoding.UTF8.GetString(input);
            _logger.LogInformation($"TryParseMessage: {s}");
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