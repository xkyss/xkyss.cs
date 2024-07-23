<Query Kind="Program">
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
	var m = new HeartBeat() {
		Uid = 11,
		TimeTick = 22,
	};


	var bytes = MessagePackSerializer.Serialize(m);
	Assert.NotNull(bytes);


	var bytes2 = MessagePackSerializer.Serialize((Message) m);
	Assert.NotNull(bytes2);
	
	Assert.NotEqual(bytes.Length, bytes2.Length);
}