using System.Collections.Concurrent;

namespace Ks.Net.Socket;

internal class SocketTypeMapper : ISocketTypeMapper
{
    private readonly ConcurrentDictionary<Type, int> _typeToId = new();
    private readonly ConcurrentDictionary<int, Type> _idToType = new();

    public bool Contains(Type type)
    {
        return _typeToId.ContainsKey(type);
    }

    public bool Contains(int id)
    {
        return _idToType.ContainsKey(id);
    }

    public bool TryGet(int id, out Type type)
    {
        return _idToType.TryGetValue(id, out type!);
    }

    public bool TrgGet(Type type, out int id)
    {
        return _typeToId.TryGetValue(type, out id);
    }

    public void Register<T>(int id)
    {
        Register(typeof(T), id);
    }

    public void Register(Type type, int id)
    {
        _idToType[id] = type;
        _typeToId[type] = id;
    }

    public void UnRegister(Type type)
    {
        if (_typeToId.TryGetValue(type, out var id))
        {
            _idToType.Remove(id, out _);
            _typeToId.Remove(type, out _);
        }
        // else
        // {
        //  // 如果TypeToId中没有找到, 但是IdToType中其实有, 那就尴尬了
        // }
    }

    public void UnRegister(int id)
    {
        if (_idToType.TryGetValue(id, out var type))
        {
            _typeToId.Remove(type, out _);
            _idToType.Remove(id, out _);
        }
        // else
        // {
        //  // 如果IdToType中没有找到, 但是TypeToId中其实有, 那就尴尬了
        // }
    }
}