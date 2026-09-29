using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComputeNexus.Service.Interface;

public interface IEmailRetryPolicy
{
    Task<bool> ExecuteAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}