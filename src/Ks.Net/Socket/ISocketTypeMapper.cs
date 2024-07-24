namespace Ks.Net.Socket;

public interface ISocketTypeMapper
{
    bool Contains(Type type);

    bool Contains(int id);

    bool TryGet(int id, out Type type);

    bool TrgGet(Type type, out int id);

    void Register<T>(int id);

    void Register(Type type, int id);

    void UnRegister(Type type);

    void UnRegister(int id);
}