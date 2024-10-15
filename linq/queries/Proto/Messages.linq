<Query Kind="Statements">
  <NuGetReference>MessagePack</NuGetReference>
  <Namespace>MessagePack</Namespace>
</Query>



[MessagePackObject]
public class Message
{
	[Key(0)]
	public long Uid { get; set; }
}


[MessagePackObject]
public class HeartBeat : Message
{
	[Key(1)]
	public long TimeTick { get; set; }
}

[MessagePackObject]
public class Request
{
	[Key(0)]
	public int TransportId {get;set;}

	[Key(1)]
	public int MessageTypeId {get;set;}
	
	[IgnoreMember]
	public Message Message {get;set;}
}
