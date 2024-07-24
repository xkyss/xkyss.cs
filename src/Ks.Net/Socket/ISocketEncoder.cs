namespace Ks.Net.Socket;

public interface ISocketEncoder
{
    void Encode<T>(Stream stream, T value);

    byte[] Encode<T>(T value);

    byte[] Encode(Type type, object? value);
}