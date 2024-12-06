using Ks.Exp.AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
   .ConfigureServices((hostContext, services) =>
    {
        services.AddAutoMapper(typeof(AutoMapperProfile));
        services.AddHostedService<HostedService>();
    })
   .UseConsoleLifetime()
   .Build();

await host.StartAsync();

Console.WriteLine("Started");

await host.StopAsync();