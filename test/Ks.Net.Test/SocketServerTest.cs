using Ks.Net.Socket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace Ks.Net.Test;

public class SocketServerTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SocketServerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestServer()
    {
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { "SocketServer:Port", "9999" }
                });
            })
            .ConfigureServices((_, services) =>
            {
                services.AddTransient<SocketServer>();
                services.AddHostedService<TestSocketServerHostedService>();
            })
            .Build()
            .Start();
        _testOutputHelper.WriteLine("Test Server Startup.");

        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { "SocketClient:Port", "9999" }
                });
            })
            .ConfigureServices((_, services) =>
            {
                services.AddTransient<SocketClient>();
                services.AddTransient<SocketClientChannel>();
                services.AddHostedService<TestSocketClientHostedService>();
            })
            .Build()
            .Start();

        _testOutputHelper.WriteLine("Test Client Startup.");
        
        System.Console.Read();
    }

    class TestSocketServerHostedService(SocketServer ss) : IHostedService
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

    class TestSocketClientHostedService(SocketClient sc) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            sc.Start();
            await Task.Delay(1000);
            await sc.Write(new Message());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}