namespace ComputeNexus.Web.Response;

public record HardwareNodeResponse(
    Guid Id,
    string Name,
    string Provider,
    string? Region,
    string GpuModel,
    int TotalGpuCount,
    int AvailableGpuCount,
    decimal PricePerGpuHour,
    bool IsActive,
    DateTime? LastHeartbeat,
    int CpuCores,
    int MemoryGb,
    int StorageGb,
    int GpuRamGb
);