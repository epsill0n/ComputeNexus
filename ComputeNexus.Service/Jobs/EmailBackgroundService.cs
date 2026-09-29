using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos.Email;
using ComputeNexus.Service.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComputeNexus.Service.Jobs;

public class EmailBackgroundService : BackgroundService
{
    private readonly Channel<EmailMessage> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        Channel<EmailMessage> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            await ProcessMessageAsync(message, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(
        EmailMessage message,
        CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider
                .GetRequiredService<IEmailService>();
            var retryPolicy = scope.ServiceProvider
                .GetRequiredService<IEmailRetryPolicy>();

            var succeeded = await retryPolicy.ExecuteAsync(
                ct => emailService.SendEmailAsync(message, ct),
                stoppingToken);

            if (succeeded)
            {
                _logger.LogInformation(
                    "Email to {To} sent successfully.", message.To);
            }
            else
            {
                _logger.LogError(
                    "Email to {To} could not be delivered after all retries.",
                    message.To);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Email background service is shutting down; " +
                "the current message will be lost.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while processing queued email to {To}.",
                message.To);
        }
    }
}