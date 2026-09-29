namespace ComputeNexus.Domain.Dtos;

public class ProviderHardwareDto
{
    public string ProviderId { get; set; } = string.Empty;
    public string GpuModel { get; set; } = string.Empty;
    public int AvailableGpuCount { get; set; }
    public int GpuRamGb { get; set; }
    public decimal PricePerGpuHour { get; set; }
    public string Region { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    
    public int CpuCores { get; set; }
    public int CpuMemoryGb { get; set; }
    public int StorageGb { get; set; }
}