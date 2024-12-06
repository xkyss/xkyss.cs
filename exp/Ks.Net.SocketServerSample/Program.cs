using Ks.Net.Socket.Extensions;
using Ks.Net.Socket.Telnet;
using Serilog;

Console.WriteLine("Hello, SocketServer!");

var builder = WebApplication.CreateBuilder(args);

builder.Services
   .AddSocketServer()
   .AddTelnetServer()
   .AddConnections();

builder.Host.UseSerilog((hosting, logger) =>
{
    logger.ReadFrom
       .Configuration(hosting.Configuration)
       .Enrich.FromLogContext().WriteTo
       .Console(
            outputTemplate:
            "{Timestamp:O} [{Level:u3}]{NewLine}{SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
});

builder.WebHost.ConfigureKestrel((context, kestrel) =>
{
    var section = context.Configuration.GetSection("Kestrel");
    kestrel.Configure(section)
       .Endpoint("SocketServer", endpoint => endpoint.ListenOptions.UseSocketServer());
});

var app = builder.Build();

app.MapConnectionHandler<TelnetConnectionHandler>("/telnet");
app.Map("/", async context =>
{
    context.Response.ContentType = "application/json;charset=utf-8";
    await context.Response.WriteAsJsonAsync(new
    {
        Code = 0,
        Message = "Hello",
    });
});
app.Run();

Console.WriteLine("Goodbye, SocketServer!");