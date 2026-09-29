namespace ComputeNexus.Domain.Dtos;

public class ComputeInstanceImportDto
{
    public string Name { get; set; } = string.Empty;
    public int GpuCount { get; set; }
    public int CpuCores { get; set; }
    public int MemoryGb { get; set; }
    public int StorageGb { get; set; }
    public Guid? TemplateId { get; set; }
    public string? UserId { get; set; }
}