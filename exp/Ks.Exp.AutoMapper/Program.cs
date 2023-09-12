
using Ks.Exp.AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostContext, services) =>
	{
		services.AddAutoMapper(typeof(AutoMapperProfile));
		services.AddHostedService<HostedService>();
	})
	.UseConsoleLifetime()
	.Build()
	.RunAsync()
	;

