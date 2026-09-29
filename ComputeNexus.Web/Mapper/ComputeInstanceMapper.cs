using ComputeNexus.Domain.Dtos;
using ComputeNexus.Service.Interface;
using ComputeNexus.Web.Extensions;
using ComputeNexus.Web.Request;
using ComputeNexus.Web.Response;

namespace ComputeNexus.Web.Mapper;

public class ComputeInstanceMapper
{
    private readonly IComputeInstanceService _instanceService;
    private readonly ICurrentUser _currentUser;

    public ComputeInstanceMapper(IComputeInstanceService instanceService, ICurrentUser currentUser)
    {
        _instanceService = instanceService;
        _currentUser = currentUser;
    }

    public async Task<List<ComputeInstanceResponse>> GetAllAsync()
    {
        var result = await _instanceService.GetAllAsync();
        return result.ToResponse();
    }

    public async Task<ComputeInstanceResponse?> GetByIdAsync(Guid id)
    {
        var result = await _instanceService.GetByIdAsync(id);
        return result?.ToResponse();
    }

    public async Task<ComputeInstanceResponse> CreateAsync(CreateComputeInstanceRequest request)
    {
        var userId = _currentUser.GetUserId() ?? throw new InvalidOperationException("User not authenticated");
        
        var result = await _instanceService.CreateAsync(
            new ComputeInstanceCreateDto()
            {
                name = request.Name,
                gpuCount = request.GpuCount,
                cpuCores = request.CpuCores,
                memoryGb = request.MemoryGb,
                storageGb = request.StorageGb,
                templateId = request.TemplateId,
                userId = userId
            });
        
        return result.ToResponse();
    }

    public async Task<ComputeInstanceResponse> UpdateAsync(Guid id, UpdateComputeInstanceRequest request)
    {
        var result = await _instanceService.UpdateAsync(
            new ComputeInstanceUpdateDto()
            {
                id = id,
                name = request.Name,
                gpuCount = request.GpuCount,
                cpuCores = request.CpuCores,
                memoryGb = request.MemoryGb,
                storageGb = request.StorageGb
            });
        
        return result.ToResponse();
    }

    public async Task<ComputeInstanceResponse> DeleteAsync(Guid id)
    {
        var result = await _instanceService.DeleteAsync(id);
        return result.ToResponse();
    }

    public async Task<ComputeInstanceResponse> StartAsync(Guid id)
    {
        var result = await _instanceService.StartAsync(id);
        return result.ToResponse();
    }

    public async Task<ComputeInstanceResponse> StopAsync(Guid id)
    {
        var result = await _instanceService.StopAsync(id);
        return result.ToResponse();
    }
    public async Task<PaginatedResponse<ComputeInstanceResponse>> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        string? filter = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        var result = await _instanceService.GetPagedAsync(
            pageNumber, pageSize, filter, sortBy, sortDescending);
    
        return result.ToPaginatedResponse(e => e.ToResponse());
    }
}