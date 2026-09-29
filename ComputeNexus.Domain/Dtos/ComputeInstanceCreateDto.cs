namespace ComputeNexus.Domain.Dtos;

public class ComputeInstanceCreateDto
{
    public string name { get; set; }
    public int gpuCount { get; set; }
    public int cpuCores { get; set; }
    public int memoryGb { get; set; }
    public int storageGb { get; set; }
    public Guid? templateId { get; set; }
    public string userId { get; set; }
}