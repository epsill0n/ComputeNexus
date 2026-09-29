namespace ComputeNexus.Web.Response;

public record JobResponse(
    Guid Id,
    string Name,
    string Status,
    string Type,
    string? EntryPoint,
    string? Arguments,
    string? OutputUrl,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    int? ExitCode,
    decimal ComputeCost,
    Guid ComputeInstanceId,
    string ComputeInstanceName,
    string UserId,
    string Username
);