using System.IO.Pipelines;

namespace Ks.Net
{
    public abstract class NetChannel
    {
        protected readonly CancellationTokenSource CloseTokenSource = new();

        public virtual Task RunAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task Write(Message message)
        {
            return Task.CompletedTask;
        }
    }
}