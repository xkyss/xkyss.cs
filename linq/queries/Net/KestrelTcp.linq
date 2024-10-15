<Query Kind="Program">
  <Namespace>Microsoft.AspNetCore.Builder</Namespace>
  <Namespace>Microsoft.AspNetCore.Connections</Namespace>
  <Namespace>Microsoft.AspNetCore.Hosting</Namespace>
  <Namespace>Microsoft.Extensions.Hosting</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>System.Net.Sockets</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

async Task Main()
{
	var port = 9991;
	await StartServer(port);
	await StartClient("localhost", port);
	System.Console.Read();
}

public async Task StartClient(string host, int port)
{
	var socket = new TcpClient(AddressFamily.InterNetwork);
	try
	{
		socket.NoDelay = true;
		await socket.ConnectAsync(host, port);
	}
	catch (Exception e)
	{
		System.Console.WriteLine($"[Client]: Started FAILED. {e.Message}");
		return;
	}
	
	System.Console.WriteLine("[Client]: Started.");
}

public Task StartServer(int port)
{
	var builder = WebApplication.CreateBuilder();
	builder.WebHost.UseKestrel(options =>
	{
		options.ListenAnyIP(port, builder =>
		{
			builder.UseConnectionHandler<TcpConnectionHandler>();
		});
	})
	.ConfigureLogging(logging =>
	{
		logging.SetMinimumLevel(LogLevel.Error);
	});

	var app = builder.Build();
	var task = app.StartAsync();

	System.Console.WriteLine("[Server]: Started.");
	return task;
}


public class TcpConnectionHandler : ConnectionHandler
{
	public override Task OnConnectedAsync(ConnectionContext connection)
	{
		System.Console.WriteLine("[Server]: OnConnectedAsync");
		System.Console.WriteLine($"\t[Server]: connection id: {connection.ConnectionId}");
		return Task.CompletedTask;
	}
}