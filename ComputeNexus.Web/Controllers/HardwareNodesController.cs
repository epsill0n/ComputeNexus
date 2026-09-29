using ComputeNexus.Web.Mapper;
using ComputeNexus.Web.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputeNexus.Web.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HardwareNodesController : ControllerBase
{
    private readonly HardwareNodeMapper _mapper;

    public HardwareNodesController(HardwareNodeMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<List<HardwareNodeResponse>> GetAll()
    {
        return await _mapper.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mapper.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("provider/{provider}")]
    public async Task<List<HardwareNodeResponse>> GetByProvider(string provider)
    {
        return await _mapper.GetByProviderAsync(provider);
    }

    [HttpGet("gpu/{gpuModel}")]
    public async Task<List<HardwareNodeResponse>> GetByGpuModel(string gpuModel)
    {
        return await _mapper.GetByGpuModelAsync(gpuModel);
    }

    [HttpGet("available")]
    public async Task<List<HardwareNodeResponse>> GetAvailable()
    {
        return await _mapper.GetAvailableAsync();
    }
    
    [HttpGet("paged")]
    public async Task<PaginatedResponse<HardwareNodeResponse>> GetPaged(
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filter = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        return await _mapper.GetPagedAsync(pageNumber, pageSize, filter, sortBy, sortDescending);
    }
}