using System;
using System.Threading.Tasks;
using ComputeNexus.Service.Interface;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ComputeNexus.Service.Jobs;

[DisallowConcurrentExecution]
public class InstanceCleanupJob : IJob
{
    private static readonly TimeSpan StuckDuration = TimeSpan.FromHours(1);

    private readonly IComputeInstanceService _instanceService;
    private readonly ILogger<InstanceCleanupJob> _logger;

    public InstanceCleanupJob(
        IComputeInstanceService instanceService,
        ILogger<InstanceCleanupJob> logger)
    {
        _instanceService = instanceService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation(
            "Instance cleanup job started (threshold: {Threshold}).",
            StuckDuration);

        try
        {
            var terminated = await _instanceService.CleanupStuckInstancesAsync(
                StuckDuration,
                context.CancellationToken);

            _logger.LogInformation(
                "Instance cleanup job finished. Terminated {Count} instances.",
                terminated);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Instance cleanup job was cancelled (application is shutting down).");
            throw new JobExecutionException(new OperationCanceledException("Job cancelled"), refireImmediately: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Instance cleanup job failed.");
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}