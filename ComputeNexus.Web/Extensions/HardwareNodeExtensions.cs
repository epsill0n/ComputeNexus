using ComputeNexus.Domain.Entities;
using ComputeNexus.Web.Response;

namespace ComputeNexus.Web.Extensions;

public static class HardwareNodeExtensions
{
    public static HardwareNodeResponse ToResponse(this HardwareNode node)
    {
        return new HardwareNodeResponse(
            node.Id,
            node.Name,
            node.Provider,
            node.Region,
            node.GpuModel,
            node.TotalGpuCount,
            node.AvailableGpuCount,
            node.PricePerGpuHour,
            node.IsActive,
            node.LastHeartbeat,
            node.CpuCores,
            node.CpuMemoryGb,
            node.StorageGb,
            node.PerGpuRamGb
        );
    }

    public static List<HardwareNodeResponse> ToResponse(this List<HardwareNode> nodes)
    {
        return nodes.Select(n => n.ToResponse()).ToList();
    }
}