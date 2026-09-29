using ComputeNexus.Domain.Enums;

namespace ComputeNexus.Domain.Entities;

public class JobLog : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public LogLevel Level { get; set; }
    public DateTime Timestamp { get; set; }
    
    // fks
    public Guid JobId { get; set; }
    public string UserId { get; set; }
    public Guid ComputeInstanceId { get; set; }
    
    // nav props
    public virtual Job Job { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ComputeInstance ComputeInstance { get; set; } = null!;
}