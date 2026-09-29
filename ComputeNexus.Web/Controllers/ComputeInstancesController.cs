using ComputeNexus.Web.Mapper;
using ComputeNexus.Web.Request;
using ComputeNexus.Web.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputeNexus.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComputeInstancesController : ControllerBase
{
    private readonly ComputeInstanceMapper _mapper;

    public ComputeInstancesController(ComputeInstanceMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<List<ComputeInstanceResponse>> GetAll()
    {
        return await _mapper.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _mapper.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComputeInstanceRequest request)
    {
        try
        {
            var result = await _mapper.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateComputeInstanceRequest request)
    {
        try
        {
            var result = await _mapper.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var result = await _mapper.DeleteAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start([FromRoute] Guid id)
    {
        try
        {
            var result = await _mapper.StartAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> Stop([FromRoute] Guid id)
    {
        try
        {
            var result = await _mapper.StopAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpGet("paged")]
    public async Task<PaginatedResponse<ComputeInstanceResponse>> GetPaged(
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filter = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        return await _mapper.GetPagedAsync(pageNumber, pageSize, filter, sortBy, sortDescending);
    }
}