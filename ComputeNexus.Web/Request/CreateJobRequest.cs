using System.ComponentModel.DataAnnotations;
using ComputeNexus.Domain.Enums;

namespace ComputeNexus.Web.Request;

public record CreateJobRequest(
    [Required] string Name,
    [Required] JobType Type,
    string? EntryPoint,
    string? Arguments,
    [Required] Guid ComputeInstanceId
);