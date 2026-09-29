using System.ComponentModel.DataAnnotations;

namespace ComputeNexus.Web.Request;

public record CreateComputeInstanceRequest
(
    [Required] string Name,
    [Required] [Range(1, 8)] int GpuCount,
    [Required] [Range(1, 64)] int CpuCores,
    [Required] [Range(1, 512)] int MemoryGb,
    [Required] [Range(10, 2000)] int StorageGb,
    Guid? TemplateId
);