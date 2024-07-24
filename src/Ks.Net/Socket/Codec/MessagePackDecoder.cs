using System.Buffers;
using MessagePack;

namespace Ks.Net.Socket.Codec;

public class MessagePackDecoder : ISocketDecoder
{
    public T Decode<T>(byte[] data)
    {
        return MessagePackSerializer.Deserialize<T>(data);
    }

    public T Decode<T>(Stream data)
    {
        return MessagePackSerializer.Deserialize<T>(data);
    }

    public T Decode<T>(ReadOnlySequence<byte> data)
    {
        return MessagePackSerializer.Deserialize<T>(data);
    }

    public object? Decode(Type type, byte[] data)
    {
        return MessagePackSerializer.Deserialize(type, data);
    }

    public object? Decode(Type type, Stream data)
    {
        return MessagePackSerializer.Deserialize(type, data);
    }

    public object? Decode(Type type, ReadOnlySequence<byte> data)
    {
        return MessagePackSerializer.Deserialize(type, data);
    }
}