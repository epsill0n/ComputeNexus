using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Entities;

namespace ComputeNexus.Service.Interface;

public interface IHardwareNodeService
{
    Task<List<HardwareNode>> GetAllAsync();
    Task<HardwareNode?> GetByIdAsync(Guid id);
    Task<List<HardwareNode>> GetByProviderAsync(string provider);
    Task<List<HardwareNode>> GetByGpuModelAsync(string gpuModel);
    Task<List<HardwareNode>> GetAvailableAsync();
    
    Task<PaginatedResult<HardwareNode>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? filter = null,
        string? sortBy = null,
        bool sortDescending = false);
}