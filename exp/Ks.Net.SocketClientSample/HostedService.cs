using Ks.Net.Socket;
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

            await socketClient.WriteLineAsync(line);
        }

        _ = lifetime.StopAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await socketClient.StopAsync();
    }
}