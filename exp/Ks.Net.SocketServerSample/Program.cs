
using Ks.Net.SocketServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Console.WriteLine("Hello, World!");

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddConnections();

builder.Host.UseSerilog((hosting, logger) =>
{
    logger.ReadFrom
        .Configuration(hosting.Configuration)
        .Enrich.FromLogContext().WriteTo.Console(outputTemplate: "{Timestamp:O} [{Level:u3}]{NewLine}{SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
});

builder.WebHost.ConfigureKestrel((context, kestrel) =>
{
    var section = context.Configuration.GetSection("Kestrel");
    kestrel.Configure(section)
        .Endpoint("SocketServer", endpoint => endpoint.ListenOptions.UseSocketServer());
});

var app = builder.Build();
app.Run();
