using System.Threading;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos.Email;

namespace ComputeNexus.Service.Interface;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message, CancellationToken ct = default);
}