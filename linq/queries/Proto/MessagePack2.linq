<Query Kind="Program">
  <NuGetReference>MessagePack</NuGetReference>
  <Namespace>MessagePack.Formatters</Namespace>
  <Namespace>MessagePack.Resolvers</Namespace>
  <Namespace>Xunit</Namespace>
</Query>


#load "xunit"
#load ".\Messages"


void Main()
{
	RunTests();
}


[Fact]
public void Test01()
{
	var m = new HeartBeat()
	{
		Uid = 111,
		TimeTick = 222,
	};
	
	var bytes = MessagePackSerializer.Serialize(m);
	Assert.NotNull(bytes);
	
	var r = new Request()
	{
		TransportId = 333,
		MessageTypeId = 444,
		Message = m,
	};
	var rbytes = MessagePackSerializer.Serialize(r);
	Assert.NotNull(rbytes);

	var r2 = MessagePackSerializer.Deserialize<Request>(rbytes);
	Assert.NotNull(r2);
	r2.Dump();

	var m3 = MessagePackSerializer.Deserialize(typeof(HeartBeat), bytes);
	m3.Dump();
}
