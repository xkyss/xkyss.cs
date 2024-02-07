using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ks.Net.Socket;

public class SocketServer
{
    private readonly WebApplication _app;
    private readonly ILogger _logger;

    public SocketServer(IConfiguration configuration, ILogger<SocketServer> logger)
    {
        _logger = logger;
        
        var serverConfig = configuration.GetSection(Constants.DefaultSocketConfigKey);
        var port = serverConfig.GetValue(Constants.DefaultSocketPortKey, Constants.DefaultSocketPort);
        
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(port, listenOptions =>
                {
                    listenOptions.UseConnectionHandler<SocketConnectionHandler>();
                });
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<SocketConnectionHandler>();
            })
            .ConfigureAppConfiguration((webHostContext, configurationBuilder) =>
            {
                configurationBuilder.AddConfiguration(serverConfig);
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
            });

        _app = builder.Build();
        _logger.LogInformation("初始化完成.");
    }
    
    public Task Start()
    {
        _logger.LogInformation("启动服务.");
        return _app.StartAsync();
    }

    public Task Stop()
    {
        _logger.LogInformation("停止服务.");
        return _app.StopAsync();
    }
}