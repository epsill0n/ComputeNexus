using System;
using System.Threading;
using System.Threading.Tasks;
using ComputeNexus.Domain.Configuration;
using ComputeNexus.Domain.Dtos.Email;
using ComputeNexus.Service.Interface;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ComputeNexus.Service.Implementation;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }
    
    
    public async Task SendEmailAsync(EmailMessage message, CancellationToken ct = default)
    {
        var email = new MimeMessage();
        
        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;


        var builder = new BodyBuilder()
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.PlainText
        };

        if (message.Attachments is { Count: > 0 })
        {
            foreach (var attachment in message.Attachments)
            {
                builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
            }
        }


        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, 
                _settings.UseSsl ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.Auto, ct);
            
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            throw;
        }
        finally
        {
            smtp.DisconnectAsync(true, ct);
        }
    }
}
