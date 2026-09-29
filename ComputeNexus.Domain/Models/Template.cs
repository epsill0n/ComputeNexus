namespace ComputeNexus.Domain.Entities;

public class Template : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string PythonVersion { get; set; } = string.Empty;
    public string CudaVersion { get; set; } = string.Empty;
    public string DockerImage { get; set; } = string.Empty;
    public int MinMemoryGb { get; set; }
    public int MinGpuCount { get; set; }
    public bool IsPublic { get; set; }
    
    public string? CreatedByUserId { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<ComputeInstance> Instances { get; set; } = new List<ComputeInstance>();
}