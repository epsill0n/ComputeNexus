namespace ComputeNexus.Web.Response;

public record ComputeInstanceResponse(
    Guid Id,
    string Name,
    string Status,
    string? AssignedGpuModel,
    int GpuCount,
    int CpuCores,
    int MemoryGb,
    int StorageGb,
    DateTime? StartedAt,
    DateTime? StoppedAt,
    decimal HourlyRate,
    decimal TotalCost,
    string UserId,
    string Username,
    Guid? HardwareNodeId,
    string HardwareNodeName,
    Guid? TemplateId,
    string TemplateName
);