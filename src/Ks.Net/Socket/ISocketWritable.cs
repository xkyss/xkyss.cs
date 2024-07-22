using System.Text;

namespace Ks.Net.Socket;

public interface ISocketWritable<out T>
{
    public Task WriteAsync(ReadOnlySpan<char> text, Encoding? encoding = null);

    public T Write(ReadOnlySpan<char> text, Encoding? encoding = null);

    public Task WriteLineAsync(ReadOnlySpan<char> text, Encoding? encoding = null);

    public T WriteLine(ReadOnlySpan<char> text, Encoding? encoding = null);

    public Task FlushAsync();
}