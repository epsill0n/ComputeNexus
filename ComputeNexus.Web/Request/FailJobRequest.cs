using System.ComponentModel.DataAnnotations;

namespace ComputeNexus.Web.Request;

public record FailJobRequest(
    [Required] string ErrorMessage
);