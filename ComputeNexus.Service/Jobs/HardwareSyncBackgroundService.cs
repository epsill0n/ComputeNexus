using System;
using System.Threading;
using System.Threading.Tasks;
using ComputeNexus.Service.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComputeNexus.Service.Jobs;

public class HardwareSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HardwareSyncBackgroundService> _logger;

    public HardwareSyncBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<HardwareSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IHardwareSyncService>();

            try
            {
                _logger.LogInformation("Starting hardware sync job");
                var result = await syncService. SyncAllAsync();
                _logger.LogInformation("Sync completed: {Success}, processed {Count}", result.Success, result.NodesProcessed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during hardware sync");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}