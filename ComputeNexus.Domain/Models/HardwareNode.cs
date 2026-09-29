namespace ComputeNexus.Domain.Entities;

public class HardwareNode : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string GpuModel { get; set; } = string.Empty;
    public int TotalGpuCount { get; set; }
    public int AvailableGpuCount { get; set; }
    public int PerGpuRamGb { get; set; }
    public int CpuCores { get; set; }
    public int CpuMemoryGb { get; set; }
    public int StorageGb { get; set; }
    public decimal PricePerGpuHour { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    
    // nav props
    public virtual ICollection<ComputeInstance> Instances { get; set; } = new List<ComputeInstance>();
}