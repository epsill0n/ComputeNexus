using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos.Email;

namespace ComputeNexus.Service.Interface;

public interface IEmailQueue
{
    Task EnqueueAsync(EmailMessage message);
}