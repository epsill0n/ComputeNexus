using System.ComponentModel.DataAnnotations;

namespace ComputeNexus.Web.Request;

public record AssignHardwareRequest(
    [Required] Guid HardwareNodeId
);