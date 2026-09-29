using System.Threading.Channels;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos.Email;
using ComputeNexus.Service.Interface;

namespace ComputeNexus.Service.Implementation;

public class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public ChannelEmailQueue(Channel<EmailMessage> channel) => _channel = channel;

    public async Task EnqueueAsync(EmailMessage message) =>
        await _channel.Writer.WriteAsync(message);
}