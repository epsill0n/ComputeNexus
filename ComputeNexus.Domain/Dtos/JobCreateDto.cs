using ComputeNexus.Domain.Enums;

namespace ComputeNexus.Domain.Dtos;

public class JobCreateDto
{
    public string name { get; set; }
    public JobType type { get; set; }
    public string? entryPoint { get; set; }
    public string? arguments { get; set; }
    public Guid computeInstanceId { get; set; }
    public string userId { get; set; }
}