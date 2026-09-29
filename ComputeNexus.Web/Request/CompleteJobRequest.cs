namespace ComputeNexus.Web.Request;

public record CompleteJobRequest(
    string? OutputUrl,
    int? ExitCode
);