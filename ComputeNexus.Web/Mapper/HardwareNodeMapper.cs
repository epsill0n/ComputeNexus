using ComputeNexus.Service.Interface;
using ComputeNexus.Web.Extensions;
using ComputeNexus.Web.Response;

namespace ComputeNexus.Web.Mapper;

public class HardwareNodeMapper
{
    private readonly IHardwareNodeService _service;

    public HardwareNodeMapper(IHardwareNodeService service)
    {
        _service = service;
    }

    public async Task<List<HardwareNodeResponse>> GetAllAsync()
    {
        var nodes = await _service.GetAllAsync();
        return nodes.ToResponse();
    }

    public async Task<HardwareNodeResponse?> GetByIdAsync(Guid id)
    {
        var node = await _service.GetByIdAsync(id);
        return node?.ToResponse();
    }

    public async Task<List<HardwareNodeResponse>> GetByProviderAsync(string provider)
    {
        var nodes = await _service.GetByProviderAsync(provider);
        return nodes.ToResponse();
    }

    public async Task<List<HardwareNodeResponse>> GetByGpuModelAsync(string gpuModel)
    {
        var nodes = await _service.GetByGpuModelAsync(gpuModel);
        return nodes.ToResponse();
    }

    public async Task<List<HardwareNodeResponse>> GetAvailableAsync()
    {
        var nodes = await _service.GetAvailableAsync();
        return nodes.ToResponse();
    }
    
    public async Task<PaginatedResponse<HardwareNodeResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? filter = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, filter, sortBy, sortDescending);
        return result.ToPaginatedResponse(n => n.ToResponse());
    }
}