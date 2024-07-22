using Ks.Net.Socket;
using Microsoft.Extensions.Hosting;

namespace Ks.Net.SocketClientSample;

internal class HostedService(Client client, IHostLifetime lifetime)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.StartAsync();
        
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

            await client.WriteLine(line);
        }

        _ = lifetime.StopAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopAsync();
    }
}