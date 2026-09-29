using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Domain.Enums;

namespace ComputeNexus.Service.Interface;

public interface IJobService
{
    Task<List<Job>> GetAllAsync();
    Task<Job?> GetByIdAsync(Guid id);
    Task<Job> GetByIdNotNullAsync(Guid id);
    Task<Job> CreateAsync(JobCreateDto dto);
    Task<Job> UpdateAsync(Guid id, string name, string? entryPoint, string? arguments);
    Task<Job> DeleteAsync(Guid id);
    Task<Job> StartJobAsync(Guid id);
    Task<Job> CompleteJobAsync(Guid id, string? outputUrl, int? exitCode);
    Task<Job> FailJobAsync(Guid id, string errorMessage);
    Task<List<Job>> GetJobsByInstanceIdAsync(Guid computeInstanceId);
}