using Ks.Core.System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Ks.Net.Socket;

sealed class SocketResponse(PipeWriter writer)
{
    public ValueTask<FlushResult> WriteAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        Write(text, encoding);
        return FlushAsync();
    }

    public SocketResponse Write(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        writer.Write(text, encoding ?? Encoding.UTF8);
        return this;
    }
    
    public ValueTask<FlushResult> WriteLineAsync(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        WriteLine(text, encoding);
        return FlushAsync();
    }

    public SocketResponse WriteLine(ReadOnlySpan<char> text, Encoding? encoding = null)
    {
        writer.Write(text, encoding ?? Encoding.UTF8);
        writer.WriteCRLF();
        return this;
    }

    public ValueTask<FlushResult> FlushAsync()
    {
        return writer.FlushAsync();
    }
    
}
