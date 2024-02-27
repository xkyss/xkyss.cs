using System.Buffers;
using MessagePack;

namespace Ks.Serializer.MessagePack
{
    public class Serializer : ISerializer
    {
        public void Serialize<T>(Stream stream, T value)
        {
            MessagePackSerializer.Serialize(stream, value);
        }
        public byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value);
        }
        public T Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data);
        }

        public T Deserialize<T>(Stream data)
        {
            return MessagePackSerializer.Deserialize<T>(data);
        }

        public T Deserialize<T>(ReadOnlySequence<byte> data)
        {
            return MessagePackSerializer.Deserialize<T>(data);
        }
    }
}