using Ks.Net.Socket;
using Ks.Net.Socket.Client;
using Microsoft.Extensions.Hosting;

namespace Ks.Net.SocketClientSample;

internal class HostedService(SocketClient socketClient, IHostLifetime lifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await socketClient.StartAsync();
        
        Console.WriteLine("输入[bye]以结束.");
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            if (line.ToLower() == "bye")
            {
                break;
            }

            socketClient.Write(new HeartBeat()
            {
                Sid = 11,
                TimeTick = 22,
            });
        }

        _ = lifetime.StopAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await socketClient.StopAsync();
    }
}