using System.Net.Sockets;
using Ks.Net.Socket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ks.Net.Test;

public class SocketServerTest
{
    [Fact]
    public void Test1()
    {
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    {"SocketServer:Port", "9999"}
                });
            })
            .ConfigureServices((_, services) =>
            {
                services.AddTransient<SocketServer>();
                services.AddHostedService<TestHostedService>();
            })
            .Build()
            .StartAsync()
            .Wait();
        
        StartClient("localhost", 9999).Wait();
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

    class TestHostedService(SocketServer ss) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ss.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}