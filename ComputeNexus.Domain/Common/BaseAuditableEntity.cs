namespace ComputeNexus.Domain.Entities;

public abstract class BaseAuditableEntity : BaseEntity
{
    public string? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedById { get; set; }
    public DateTime? ModifiedAt { get; set; }
}