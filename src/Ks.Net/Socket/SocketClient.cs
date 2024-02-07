using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket;

public class SocketClient
{
    private readonly SocketClientChannel _channel;
    private readonly ILogger _logger;
    private readonly string _ip;
    private readonly int _port;

    public SocketClient(IConfiguration configuration, ILogger<SocketClient> logger, SocketClientChannel channel)
    {
        _logger = logger;
        _channel = channel;
        
        var serverConfig = configuration.GetSection(Constants.DefaultSocketClientKey);
        _ip = serverConfig.GetValue(Constants.DefaultSocketHostKey, Constants.DefaultSocketHost)!;
        _port = serverConfig.GetValue(Constants.DefaultSocketPortKey, Constants.DefaultSocketPort);
    }

    public Task Write(Message message)
    {
        return _channel.Write(message);
    }
    
    public async Task Start()
    {
        _logger.LogInformation("启动客户端.");
        await _channel.ConnectAsync(_ip, _port);
        await _channel.RunAsync();
    }

    public Task Stop()
    {
        _logger.LogInformation("停止客户端.");
        return Task.CompletedTask;
    }
}