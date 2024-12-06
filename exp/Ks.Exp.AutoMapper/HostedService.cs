using AutoMapper;
using Ks.Exp.AutoMapper.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ks.Exp.AutoMapper;

internal class HostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IHostApplicationLifetime _lifetime;

    public HostedService(IMapper mapper, IHostApplicationLifetime lifetime, ILogger<HostedService> logger)
    {
        _mapper = mapper;
        _lifetime = lifetime;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StartAsync");

        Test();

        // _lifetime.StopApplication();
        return Task.CompletedTask;
    }

    private void Test()
    {
        var bar = new BarModel()
        {
            Id = 1,
            Name = "Bar",
        };

        var barVm = _mapper.Map<BarViewModel>(bar);

        _logger.LogInformation("BarViewModel: {vm}", barVm);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StopAsync");
        return Task.CompletedTask;
    }
}