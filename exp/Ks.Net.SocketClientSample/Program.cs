using Ks.Net.Socket;
using Ks.Net.Socket.Client;
using Ks.Net.Socket.Extensions;
using Serilog.Events;
using Serilog;
using Ks.Net.SocketClientSample;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var folder = genSpecialFolder();
Console.WriteLine($"App Folder: {folder}");

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
	.MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting console host.");

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        var env = context.HostingEnvironment;
        var contentRootPath = context.HostingEnvironment.ContentRootPath;

        Log.Information(" EnvironmentName: {env}", env.EnvironmentName);

        builder.SetBasePath(folder);
        builder.AddJsonFile("appsettings.json", true);
        builder.AddJsonFile(Path.Combine(contentRootPath, "appsettings.json"), true);
        builder.AddJsonFile(Path.Combine(contentRootPath, $"appsettings.{env.EnvironmentName}.json"), true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<HostedService>();
        services.AddSocketClient();
    })
    .UseConsoleLifetime()
    .Build()
    .RunAsync()
    ;
    
    
    
static string genSpecialFolder()
{
    // 获取当前用户的用户文件夹路径
    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    string specificPath = Path.Combine(path, ".xky", "socket-client");
    // 如果子文件夹不存在，则创建它
    if (!Directory.Exists(specificPath))
    {
        Directory.CreateDirectory(specificPath);
    }
    return specificPath;
}
