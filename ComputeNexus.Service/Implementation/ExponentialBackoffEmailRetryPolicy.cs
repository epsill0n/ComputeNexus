using System;
using System.Threading;
using System.Threading.Tasks;
using ComputeNexus.Service.Interface;
using Microsoft.Extensions.Logging;

namespace ComputeNexus.Service.Implementation;

public class ExponentialBackoffEmailRetryPolicy : IEmailRetryPolicy
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(2);

    private readonly ILogger<ExponentialBackoffEmailRetryPolicy> _logger;

    public ExponentialBackoffEmailRetryPolicy(
        ILogger<ExponentialBackoffEmailRetryPolicy> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        var delay = InitialDelay;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await action(cancellationToken);
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Email send cancelled during shutdown.");
                throw;
            }
            catch (Exception ex)
            {
                if (attempt >= MaxAttempts)
                {
                    _logger.LogError(ex,
                        "Email send failed after {MaxAttempts} attempts. Giving up.",
                        MaxAttempts);
                    return false;
                }

                _logger.LogWarning(ex,
                    "Email send failed on attempt {Attempt}/{MaxAttempts}. " +
                    "Retrying in {Delay}...",
                    attempt, MaxAttempts, delay);

                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }

                delay *= 2;
            }
        }

        return false;
    }
}