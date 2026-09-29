using ComputeNexus.Domain.Entities;
using ComputeNexus.Web.Response;

namespace ComputeNexus.Web.Extensions;

public static class ComputeInstanceExtensions
{
    public static ComputeInstanceResponse ToResponse(this ComputeInstance instance)
    {
        return new ComputeInstanceResponse(
            instance.Id,
            instance.Name,
            instance.Status.ToString(),
            instance.AssignedGpuModel,
            instance.GpuCount,
            instance.CpuCores,
            instance.MemoryGb,
            instance.StorageGb,
            instance.StartedAt,
            instance.StoppedAt,
            instance.HourlyRate,
            instance.TotalCost,
            instance.UserId,
            instance.User?.UserName ?? "Unknown",
            instance.HardwareNodeId,
            instance.HardwareNode?.Name ?? "Not Assigned",
            instance.TemplateId,
            instance.Template?.Name ?? "Custom"
        );
    }

    public static List<ComputeInstanceResponse> ToResponse(this List<ComputeInstance> instances)
    {
        return instances.Select(i => i.ToResponse()).ToList();
    }
}