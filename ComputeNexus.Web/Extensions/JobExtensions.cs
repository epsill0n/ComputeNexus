using ComputeNexus.Domain.Entities;
using ComputeNexus.Web.Response;

namespace ComputeNexus.Web.Extensions;

public static class JobExtensions
{
    public static JobResponse ToResponse(this Job job)
    {
        return new JobResponse(
            job.Id,
            job.Name,
            job.Status.ToString(),
            job.Type.ToString(),
            job.EntryPoint,
            job.Arguments,
            job.OutputUrl,
            job.StartedAt,
            job.CompletedAt,
            job.ErrorMessage,
            job.ExitCode,
            job.ComputeCost,
            job.ComputeInstanceId,
            job.ComputeInstance?.Name ?? "Unknown",
            job.UserId,
            job.User?.UserName ?? "Unknown"
        );
    }

    public static List<JobResponse> ToResponse(this List<Job> jobs)
    {
        return jobs.Select(j => j.ToResponse()).ToList();
    }
}