namespace ComputeNexus.Domain.Dtos;

public class ComputeInstanceUpdateDto
{
    public Guid id { get; set; }
    public string name { get; set; }
    public int gpuCount { get; set; }
    public int cpuCores { get; set; }
    public int memoryGb { get; set; }
    public int storageGb { get; set; }
}