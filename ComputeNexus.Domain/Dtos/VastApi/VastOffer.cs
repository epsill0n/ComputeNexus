using System.Text.Json.Serialization;

namespace ComputeNexus.Domain.Dtos.VastApi;

public class VastOffer
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("gpu_name")]
    public string GpuName { get; set; } = string.Empty;

    [JsonPropertyName("num_gpus")]
    public int NumGpus { get; set; }

    [JsonPropertyName("cpu_cores")]
    public int CpuCores { get; set; }

    [JsonPropertyName("cpu_ram")]
    public int CpuRamMb { get; set; }

    [JsonPropertyName("disk_space")]
    public decimal DiskSpaceGb { get; set; }

    [JsonPropertyName("dph_total")]
    public decimal DphTotal { get; set; }

    [JsonPropertyName("geolocation")]
    public string Geolocation { get; set; } = string.Empty;

    [JsonPropertyName("reliability")]
    public decimal Reliability { get; set; }

    [JsonPropertyName("gpu_ram")]
    public int GpuRamMb { get; set; }

    [JsonPropertyName("total_flops")]
    public decimal TotalFlops { get; set; }

    [JsonPropertyName("dlperf")]
    public decimal Dlperf { get; set; }
}