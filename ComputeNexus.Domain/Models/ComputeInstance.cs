using ComputeNexus.Domain.Enums;

namespace ComputeNexus.Domain.Entities;

public class ComputeInstance : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public InstanceStatus Status { get; set; }
    public string? AssignedGpuModel { get; set; }
    public int GpuCount { get; set; }
    public int CpuCores { get; set; }
    public int MemoryGb { get; set; }
    public int StorageGb { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalCost { get; set; }
    public decimal? EstimatedMonthlyCost { get; set; }
    
    // foreign Keys
    public string UserId { get; set; } // Guid -> string
    public Guid? HardwareNodeId { get; set; }
    public Guid? TemplateId { get; set; }
    
    // navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual HardwareNode? HardwareNode { get; set; }
    public virtual Template? Template { get; set; }
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    public virtual ICollection<JobLog> JobLogs { get; set; } = new List<JobLog>();
}