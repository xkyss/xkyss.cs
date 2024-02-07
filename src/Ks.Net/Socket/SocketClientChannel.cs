using System.Buffers;
using System.IO.Pipelines;

namespace Ks.Net.Socket
{
    public class SocketClientChannel : NetChannel
    {
        private readonly Pipe _receivePipe = new();

        public override async Task RunAsync()
        {
            try
            {
                // _ = recvNetData();
                var cancelToken = CloseTokenSource.Token;
                while (!cancelToken.IsCancellationRequested)
                {
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
                // LOGGER.Error(e.Message);
            }
        }

        protected virtual bool TryParseMessage(ref ReadOnlySequence<byte> input)
        {
            return true;
        }
    }
}