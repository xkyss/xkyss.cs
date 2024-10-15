using System.Buffers;

namespace Ks.Net.Socket;

public interface ISocketDecoder
{
    T Decode<T>(byte[] data);

    T Decode<T>(Stream data);

    T Decode<T>(ReadOnlySequence<byte> data);

    object? Decode(Type type, byte[] data);

    object? Decode(Type type, Stream data);

    object? Decode(Type type, ReadOnlySequence<byte> data);
}