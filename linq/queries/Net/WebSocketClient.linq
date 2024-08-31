<Query Kind="Statements">
  <NuGetReference>MessagePack</NuGetReference>
  <Namespace>System.Net.WebSockets</Namespace>
  <Namespace>MessagePack</Namespace>
</Query>


using var ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri("ws://localhost:8760/ws"), CancellationToken.None);
var buffer = new byte[256];
while (ws.State == WebSocketState.Open)
{
	await ws.SendAsync(m, WebSocketMessageType.Binary, true, CancellationToken.None);
	var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
	if (result.MessageType == WebSocketMessageType.Close)
	{
		await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
	}
	else
	{
		Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, result.Count));
	}
}

[MessagePackObject]
public sealed class SocketRequest
{
	public static SocketRequest Empty = new();

	[Key(0)]
	public long TransportId { get; set; }

	[Key(1)]
	public int MessageTypeId { get; set; }

	[Key(2)]
	public int MessageLength { get; set; }

	[IgnoreMember]
	public object? Message { get; set; }
}

[MessagePackObject]
public class HeartBeat
{
	[Key(0)]
	public int Sid { get; set; }

	[Key(1)]
	public int TimeTick { get; set; }

	public override string ToString()
	{
		return $"[{GetType()}]: Sid: {Sid}, TimeTick: {TimeTick}";
	}
}