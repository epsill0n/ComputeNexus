using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Entities;

namespace ComputeNexus.Service.Interface;

public interface IComputeInstanceService
{
    Task<List<ComputeInstance>> GetAllAsync();
    Task<ComputeInstance?> GetByIdAsync(Guid id);
    Task<ComputeInstance> GetByIdNotNullAsync(Guid id);
    Task<ComputeInstance> CreateAsync(ComputeInstanceCreateDto dto);
    Task<ComputeInstance> UpdateAsync(ComputeInstanceUpdateDto dto);
    Task<ComputeInstance> DeleteAsync(Guid id);
    Task<ComputeInstance> StartAsync(Guid id);
    Task<ComputeInstance> StopAsync(Guid id);
    Task<ComputeInstance> AssignHardwareAsync(Guid instanceId, Guid hardwareNodeId);
    Task<PaginatedResult<ComputeInstance>> GetPagedAsync(int pageNumber, 
        int pageSize, 
        string? filter = null,
        string? sortBy = null,
        bool sortDescending = false);
    
    Task<int> CleanupStuckInstancesAsync(
        TimeSpan stuckDuration,
        CancellationToken cancellationToken = default);
}