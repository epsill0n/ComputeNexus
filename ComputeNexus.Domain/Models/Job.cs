using ComputeNexus.Domain.Enums;

namespace ComputeNexus.Domain.Entities;

public class Job : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public JobType Type { get; set; }
    public string? EntryPoint { get; set; } // script path
    public string? Arguments { get; set; }
    public string? OutputUrl { get; set; } // S3/blob storage URL for results
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ExitCode { get; set; }
    public decimal ComputeCost { get; set; }
    
    // fks
    public Guid ComputeInstanceId { get; set; }
    public string UserId { get; set; }
    
    // nav props
    public virtual ComputeInstance ComputeInstance { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<JobLog> JobLogs { get; set; } = new List<JobLog>();
}