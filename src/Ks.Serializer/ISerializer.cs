using System.Buffers;

namespace Ks.Serializer;

public interface ISerializer
{
    public void Serialize<T>(Stream stream, T value);

    public byte[] Serialize<T>(T value);

    public T Deserialize<T>(byte[] data);

    public T Deserialize<T>(Stream data);

    public T Deserialize<T>(ReadOnlySequence<byte> data);
}