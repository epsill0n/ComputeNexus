using System;
using System.Linq;
using System.Threading.Tasks;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Domain.Results;
using ComputeNexus.Repository.Interface;
using ComputeNexus.Service.Interface;
using Microsoft.Extensions.Logging;

namespace ComputeNexus.Service.Implementation;

public class HardwareSyncService : IHardwareSyncService
{
    private readonly IHardwareNodeRepository _hardwareRepo;
    private readonly IProviderApiClient _providerApi;
    private readonly ILogger<HardwareSyncService> _logger;

    public HardwareSyncService(IHardwareNodeRepository hardwareRepo, IProviderApiClient providerApi, ILogger<HardwareSyncService> logger)
    {
        _hardwareRepo = hardwareRepo;
        _providerApi = providerApi;
        _logger = logger;
    }

    public async Task<SyncResult> SyncAllAsync()
    {
        var result = new SyncResult { StartedAt = DateTime.UtcNow };
        try
        {
            var providerData = await _providerApi.GetAvailableHardwareAsync();

            var nodes = providerData.Select(dto => new HardwareNode
            {
                Id = Guid.NewGuid(),
                Name = $"{dto.ProviderId} - {dto.GpuModel}",
                Provider = dto.ProviderId,
                Region = dto.Region,
                GpuModel = dto.GpuModel,
                TotalGpuCount = dto.AvailableGpuCount,
                AvailableGpuCount = dto.AvailableGpuCount,
                PricePerGpuHour = dto.PricePerGpuHour,
                IsActive = true,
                LastHeartbeat = DateTime.UtcNow,
                CpuCores = dto.CpuCores,
                CpuMemoryGb = dto.CpuMemoryGb,
                StorageGb = dto.StorageGb,
                PerGpuRamGb = dto.GpuRamGb
            }).ToList();
            
            await _hardwareRepo.BulkInsertOrUpdateAsync(nodes);

            result.Success = true;
            result.NodesProcessed = nodes.Count;
            result.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Hardware sync failed");
        }
        return result;
    }
}