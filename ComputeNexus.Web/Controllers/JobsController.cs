using ComputeNexus.Web.Mapper;
using ComputeNexus.Web.Request;
using ComputeNexus.Web.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputeNexus.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly JobMapper _mapper;

    public JobsController(JobMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<List<JobResponse>> GetAll()
    {
        return await _mapper.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mapper.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request)
    {
        var result = await _mapper.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobRequest request)
    {
        var result = await _mapper.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mapper.DeleteAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await _mapper.StartAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteJobRequest request)
    {
        var result = await _mapper.CompleteAsync(id, request.OutputUrl, request.ExitCode);
        return Ok(result);
    }

    [HttpPost("{id}/fail")]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailJobRequest request)
    {
        var result = await _mapper.FailAsync(id, request.ErrorMessage);
        return Ok(result);
    }

    [HttpGet("instance/{computeInstanceId}")]
    public async Task<List<JobResponse>> GetByInstanceId(Guid computeInstanceId)
    {
        return await _mapper.GetByInstanceIdAsync(computeInstanceId);
    }
}