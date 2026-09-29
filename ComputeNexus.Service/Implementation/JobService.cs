using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Domain.Enums;
using ComputeNexus.Repository.Interface;
using ComputeNexus.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ComputeNexus.Service.Implementation;

public class JobService : IJobService
{
    private readonly IRepository<Job> _jobRepository;
    private readonly IRepository<ComputeInstance> _instanceRepository;

    public JobService(IRepository<Job> jobRepository, IRepository<ComputeInstance> instanceRepository)
    {
        _jobRepository = jobRepository;
        _instanceRepository = instanceRepository;
    }

    public async Task<List<Job>> GetAllAsync()
    {
        var result = await _jobRepository.GetAllAsync(
            selector: x => x,
            include: x => x
                .Include(j => j.ComputeInstance)
                .Include(j => j.User));
        return result.ToList();
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        return await _jobRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id,
            include: x => x
                .Include(j => j.ComputeInstance)
                .Include(j => j.User));
    }

    public async Task<Job> GetByIdNotNullAsync(Guid id)
    {
        var result = await GetByIdAsync(id);
        if (result == null)
            throw new InvalidOperationException($"Job with id {id} not found");
        return result;
    }

    public async Task<Job> CreateAsync(JobCreateDto dto)
    {
        var instance = await _instanceRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == dto.computeInstanceId);
        
        if (instance == null)
            throw new InvalidOperationException($"ComputeInstance with id {dto.computeInstanceId} not found");

        var job = new Job
        {
            Name = dto.name,
            Status = JobStatus.Queued,
            Type = dto.type,
            EntryPoint = dto.entryPoint,
            Arguments = dto.arguments,
            ComputeInstanceId = dto.computeInstanceId,
            UserId = dto.userId,
            ComputeCost = 0
        };

        return await _jobRepository.InsertAsync(job);
    }

    public async Task<Job> UpdateAsync(Guid id, string name, string? entryPoint, string? arguments)
    {
        var job = await GetByIdNotNullAsync(id);
        
        job.Name = name;
        job.EntryPoint = entryPoint;
        job.Arguments = arguments;

        return await _jobRepository.UpdateAsync(job);
    }

    public async Task<Job> DeleteAsync(Guid id)
    {
        var job = await GetByIdNotNullAsync(id);
        return await _jobRepository.DeleteAsync(job);
    }

    public async Task<Job> StartJobAsync(Guid id)
    {
        var job = await GetByIdNotNullAsync(id);
        
        if (job.Status != JobStatus.Queued && job.Status != JobStatus.Preparing)
            throw new InvalidOperationException($"Cannot start job in {job.Status} state");

        job.Status = JobStatus.Running;
        job.StartedAt = DateTime.UtcNow;

        return await _jobRepository.UpdateAsync(job);
    }

    public async Task<Job> CompleteJobAsync(Guid id, string? outputUrl, int? exitCode)
    {
        var job = await GetByIdNotNullAsync(id);

        if (job.Status != JobStatus.Running)
            throw new InvalidOperationException($"Cannot complete job in {job.Status} state");

        job.Status = JobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.OutputUrl = outputUrl;
        job.ExitCode = exitCode;

        if (job.StartedAt.HasValue && job.CompletedAt.HasValue)
        {
            var hours = (job.CompletedAt.Value - job.StartedAt.Value).TotalHours;
            job.ComputeCost = (decimal)hours * job.ComputeInstance.HourlyRate;
        }

        return await _jobRepository.UpdateAsync(job);
    }

    public async Task<Job> FailJobAsync(Guid id, string errorMessage)
    {
        var job = await GetByIdNotNullAsync(id);

        job.Status = JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        job.ErrorMessage = errorMessage;
        job.ExitCode = 1;

        return await _jobRepository.UpdateAsync(job);
    }

    public async Task<List<Job>> GetJobsByInstanceIdAsync(Guid computeInstanceId)
    {
        var result = await _jobRepository.GetAllAsync(
            selector: x => x,
            predicate: x => x.ComputeInstanceId == computeInstanceId,
            include: x => x.Include(j => j.User));
        return result.ToList();
    }
}