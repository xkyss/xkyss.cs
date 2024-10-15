using MessagePack;

namespace Ks.Net.Socket.Codec;

public class MessagePackEncoder : ISocketEncoder
{
    public void Encode<T>(Stream stream, T value)
    {
        MessagePackSerializer.Serialize(stream, value);
    }

    public byte[] Encode<T>(T value)
    {
        return MessagePackSerializer.Serialize(value);
    }
    
    public void Encode(Type type, Stream stream, object? value)
    {
        MessagePackSerializer.Serialize(type, stream, value);
    }
    
    public byte[] Encode(Type type, object? value)
    {
        return MessagePackSerializer.Serialize(type, value);
    }
}