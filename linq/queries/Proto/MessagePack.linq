<Query Kind="Program">
  <NuGetReference>MessagePack</NuGetReference>
  <Namespace>Xunit</Namespace>
  <Namespace>MessagePack.Formatters</Namespace>
  <Namespace>MessagePack.Resolvers</Namespace>
</Query>


#load "xunit"
#load ".\Messages"


void Main()
{
	MessageResolver.Instance.Init();
	RunTests();
}


/// <summary>序列化</summary>
[Fact]
public void Test01()
{
	var m = new Message() {
		Uid = 111,
	};
	
	var bytes = MessagePackSerializer.Serialize(m);
	//bytes.ToHexString().Dump();
	//bytes.Dump();
	Assert.NotNull(bytes);
}

/// <summary>反序列化</summary>
[Fact]
public void Test02()
{
	var m = new Message()
	{
		Uid = 111,
	};

	var bytes = MessagePackSerializer.Serialize(m);

	var m2 = MessagePackSerializer.Deserialize<Message>(bytes);
	Assert.Equal(m.Uid, m2.Uid);
}

/// <summary>反序列化(继承)</summary>
[Fact]
public void Test03()
{
	var m = new HeartBeat() {
		Uid = 111,
		TimeTick = 222,
	};

	var bytes = MessagePackSerializer.Serialize(m);
	//bytes.Dump();
	Assert.NotNull(bytes);

	var m2 = MessagePackSerializer.Deserialize<HeartBeat>(bytes);
	Assert.Equal(m.Uid, m2.Uid);
	Assert.Equal(m.TimeTick, m2.TimeTick);


	var m3 = MessagePackSerializer.Deserialize<Message>(bytes);
	//m3.Dump();
	Assert.Equal(m.Uid, m3.Uid);
	
	var m4 = m3 as HeartBeat;
	Assert.Null(m4);
}

[Fact]
public void Test04()
{
	var m = new HeartBeat()
	{
		Uid = 111,
		TimeTick = 222,
	};

	var bytes = MessagePackSerializer.Serialize(m);
	//bytes.Dump();
	Assert.NotNull(bytes);

	var m3 = MessagePackSerializer.Deserialize<Message>(bytes);
	//m3.Dump();
	Assert.Equal(m.Uid, m3.Uid);

}

public class HeartBeatFormatter : IMessagePackFormatter<HeartBeat>
{
	public HeartBeat Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public void Serialize(ref MessagePackWriter writer, HeartBeat value, MessagePackSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}


public class MessageResolver : IFormatterResolver
{
	public static MessageResolver Instance { get; private set; } = new ();

	static IFormatterResolver InnerResolver;
	static List<IFormatterResolver> innerResolver = new()
	{
	   //FormatterExtensionResolver.Instance,
	   BuiltinResolver.Instance,
	   StandardResolver.Instance,
	   ContractlessStandardResolver.Instance
	};
	
	public void Init()
	{
		StaticCompositeResolver.Instance.Register(innerResolver.ToArray());
		InnerResolver = StaticCompositeResolver.Instance;
		MessagePackSerializer.DefaultOptions = new MessagePackSerializerOptions(MessageResolver.Instance);
	}
	
	public IMessagePackFormatter<T> GetFormatter<T>()
	{
		if (typeof(T)==typeof(HeartBeat)) 
		{
			return (IMessagePackFormatter<T>) new HeartBeatFormatter();
		}
		return InnerResolver.GetFormatter<T>();
	}
}
