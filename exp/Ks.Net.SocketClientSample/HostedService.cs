using Ks.Net.Socket;
using Microsoft.Extensions.Hosting;

namespace Ks.Net.SocketClientSample;

internal class HostedService(ISocketClient socketClient, IHostLifetime lifetime)
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

            int.TryParse(line, out var n);
            await socketClient.WriteAsync(new HeartBeat()
            {
                Sid = n,
                TimeTick = n,
            });
        }

        _ = lifetime.StopAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await socketClient.StopAsync();
    }
}