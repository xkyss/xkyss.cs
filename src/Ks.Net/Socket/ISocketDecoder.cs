using System.Buffers;

namespace Ks.Net.Socket;

public interface ISocketDecoder
{
    T Decode<T>(byte[] data);

    T Decode<T>(Stream data);

    T Decode<T>(ReadOnlySequence<byte> data);
}