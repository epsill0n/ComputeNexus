using ComputeNexus.Domain.Dtos;
using ComputeNexus.Service.Interface;
using ComputeNexus.Web.Extensions;
using ComputeNexus.Web.Request;
using ComputeNexus.Web.Response;

namespace ComputeNexus.Web.Mapper;

public class JobMapper
{
    private readonly IJobService _jobService;
    private readonly ICurrentUser _currentUser;

    public JobMapper(IJobService jobService, ICurrentUser currentUser)
    {
        _jobService = jobService;
        _currentUser = currentUser;
    }

    public async Task<List<JobResponse>> GetAllAsync()
    {
        var result = await _jobService.GetAllAsync();
        return result.ToResponse();
    }

    public async Task<JobResponse?> GetByIdAsync(Guid id)
    {
        var result = await _jobService.GetByIdAsync(id);
        return result?.ToResponse();
    }

    public async Task<JobResponse> CreateAsync(CreateJobRequest request)
    {
        var userId = _currentUser.GetUserId() 
            ?? throw new InvalidOperationException("User not authenticated");

        var dto = new JobCreateDto
        {
            name = request.Name,
            type = request.Type,
            entryPoint = request.EntryPoint,
            arguments = request.Arguments,
            computeInstanceId = request.ComputeInstanceId,
            userId = userId
        };

        var result = await _jobService.CreateAsync(dto);
        return result.ToResponse();
    }

    public async Task<JobResponse> UpdateAsync(Guid id, UpdateJobRequest request)
    {
        var result = await _jobService.UpdateAsync(id, request.Name, request.EntryPoint, request.Arguments);
        return result.ToResponse();
    }

    public async Task<JobResponse> DeleteAsync(Guid id)
    {
        var result = await _jobService.DeleteAsync(id);
        return result.ToResponse();
    }

    public async Task<JobResponse> StartAsync(Guid id)
    {
        var result = await _jobService.StartJobAsync(id);
        return result.ToResponse();
    }

    public async Task<JobResponse> CompleteAsync(Guid id, string? outputUrl, int? exitCode)
    {
        var result = await _jobService.CompleteJobAsync(id, outputUrl, exitCode);
        return result.ToResponse();
    }

    public async Task<JobResponse> FailAsync(Guid id, string errorMessage)
    {
        var result = await _jobService.FailJobAsync(id, errorMessage);
        return result.ToResponse();
    }

    public async Task<List<JobResponse>> GetByInstanceIdAsync(Guid computeInstanceId)
    {
        var result = await _jobService.GetJobsByInstanceIdAsync(computeInstanceId);
        return result.ToResponse();
    }
}