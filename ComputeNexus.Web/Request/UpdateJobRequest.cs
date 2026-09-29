using System.ComponentModel.DataAnnotations;

namespace ComputeNexus.Web.Request;

public record UpdateJobRequest(
    [Required] string Name,
    string? EntryPoint,
    string? Arguments
);
