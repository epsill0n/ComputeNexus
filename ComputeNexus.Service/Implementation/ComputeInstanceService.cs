using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Domain.Enums;
using ComputeNexus.Repository.Interface;
using ComputeNexus.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComputeNexus.Service.Implementation;

public class ComputeInstanceService : IComputeInstanceService
{
    private readonly IRepository<ComputeInstance> _repository;
    private readonly IRepository<HardwareNode> _hardwareRepository;
    private readonly ILogger<ComputeInstanceService> _logger;

    public ComputeInstanceService(IRepository<ComputeInstance> repository, IRepository<HardwareNode> hardwareRepository, ILogger<ComputeInstanceService> logger)
    {
        _repository = repository;
        _hardwareRepository = hardwareRepository;
        _logger = logger;
    }
    
    public async Task<List<ComputeInstance>> GetAllAsync()
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            include: x => x
                .Include(i => i.User)
                .Include(i => i.HardwareNode)
                .Include(i => i.Template));
        return result.ToList();
    }

    public async Task<ComputeInstance?> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id,
            include: x => x
                .Include(i => i.User)
                .Include(i => i.HardwareNode)
                .Include(i => i.Template)
                .Include(i => i.Jobs));
    }

    public async Task<ComputeInstance> GetByIdNotNullAsync(Guid id)
    {
        var result = await GetByIdAsync(id);
        if (result == null)
            throw new InvalidOperationException($"ComputeInstance with id {id} not found");
        return result;
    }

    public async Task<ComputeInstance> CreateAsync(ComputeInstanceCreateDto dto)
    {
        var instance = new ComputeInstance
        {
            Name = dto.name,
            Status = InstanceStatus.Pending,
            GpuCount = dto.gpuCount,
            CpuCores = dto.cpuCores,
            MemoryGb = dto.memoryGb,
            StorageGb = dto.storageGb,
            TemplateId = dto.templateId,
            UserId = dto.userId,
            HourlyRate = CalculateHourlyRate(dto.gpuCount, dto.cpuCores, dto.memoryGb),
            TotalCost = 0
        };

        return await _repository.InsertAsync(instance);
    }

    public async Task<ComputeInstance> UpdateAsync(ComputeInstanceUpdateDto dto)
    {
        var instance = await GetByIdNotNullAsync(dto.id);
        
        instance.Name = dto.name;
        instance.GpuCount = dto.gpuCount;
        instance.CpuCores = dto.cpuCores;
        instance.MemoryGb = dto.memoryGb;
        instance.StorageGb = dto.storageGb;
        instance.HourlyRate = CalculateHourlyRate(dto.gpuCount, dto.cpuCores, dto.memoryGb);

        return await _repository.UpdateAsync(instance);
    }

    public async Task<ComputeInstance> DeleteAsync(Guid id)
    {
        var instance = await GetByIdNotNullAsync(id);
        return await _repository.DeleteAsync(instance);
    }

    public async Task<ComputeInstance> StartAsync(Guid id)
    {
        var instance = await GetByIdNotNullAsync(id);
        
        if (instance.Status != InstanceStatus.Stopped && instance.Status != InstanceStatus.Pending)
            throw new InvalidOperationException($"Cannot start instance in {instance.Status} state");
        
        instance.Status = InstanceStatus.Starting;
        instance.StartedAt = DateTime.UtcNow;
        
        return await _repository.UpdateAsync(instance);
    }

    public async Task<ComputeInstance> StopAsync(Guid id)
    {
        var instance = await GetByIdNotNullAsync(id);
        
        if (instance.Status != InstanceStatus.Running)
            throw new InvalidOperationException($"Cannot stop instance in {instance.Status} state");
        
        instance.Status = InstanceStatus.Stopping;
        instance.StoppedAt = DateTime.UtcNow;
        
        return await _repository.UpdateAsync(instance);
    }

    public async Task<ComputeInstance> AssignHardwareAsync(Guid instanceId, Guid hardwareNodeId)
    {
        var instance = await GetByIdNotNullAsync(instanceId);
        var hardware = await _hardwareRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == hardwareNodeId);
        
        if (hardware == null)
            throw new InvalidOperationException($"HardwareNode with id {hardwareNodeId} not found");

        if (hardware.AvailableGpuCount < instance.GpuCount)
            throw new InvalidOperationException($"Not enough GPUs available on {hardware.Name}");

        instance.HardwareNodeId = hardwareNodeId;
        instance.AssignedGpuModel = hardware.GpuModel;
        instance.Status = InstanceStatus.Running;

        // update available GPU count
        hardware.AvailableGpuCount -= instance.GpuCount;
        await _hardwareRepository.UpdateAsync(hardware);

        return await _repository.UpdateAsync(instance);
    }

    private decimal CalculateHourlyRate(int gpuCount, int cpuCores, int memoryGb)
    {
        // simple pricing model
        decimal baseRate = 0.50m;
        decimal gpuRate = gpuCount * 1.00m;
        decimal cpuRate = cpuCores * 0.01m;
        decimal memoryRate = memoryGb * 0.002m;
        
        return baseRate + gpuRate + cpuRate + memoryRate;
    }
    
    public async Task<PaginatedResult<ComputeInstance>> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        string? filter = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        // build predicate for filtering
        Expression<Func<ComputeInstance, bool>>? predicate = null;
        if (!string.IsNullOrEmpty(filter))
        {
            predicate = x => x.Name.Contains(filter) || 
                             x.Status.ToString().Contains(filter) ||
                             x.User.UserName.Contains(filter);
        }

        // build orderBy
        Func<IQueryable<ComputeInstance>, IOrderedQueryable<ComputeInstance>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = sortBy.ToLower() switch
            {
                "name" => sortDescending 
                    ? q => q.OrderByDescending(x => x.Name) 
                    : q => q.OrderBy(x => x.Name),
                "status" => sortDescending 
                    ? q => q.OrderByDescending(x => x.Status) 
                    : q => q.OrderBy(x => x.Status),
                "hourlyrate" => sortDescending 
                    ? q => q.OrderByDescending(x => x.HourlyRate) 
                    : q => q.OrderBy(x => x.HourlyRate),
                "createdat" => sortDescending 
                    ? q => q.OrderByDescending(x => x.CreatedAt) 
                    : q => q.OrderBy(x => x.CreatedAt),
                _ => sortDescending 
                    ? q => q.OrderByDescending(x => x.CreatedAt) 
                    : q => q.OrderBy(x => x.CreatedAt)
            };
        }

        return await _repository.GetAllPagedAsync(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: predicate,
            orderBy: orderBy,
            include: x => x
                .Include(i => i.User)
                .Include(i => i.HardwareNode)
                .Include(i => i.Template),
            asNoTracking: true
        );
    }
    
    public async Task<int> CleanupStuckInstancesAsync(
        TimeSpan stuckDuration,
        CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow - stuckDuration;

        var stuck = await _repository.GetAllAsync(
            selector: x => x,
            predicate: x =>
                (x.Status == InstanceStatus.Starting ||
                 x.Status == InstanceStatus.Stopping ||
                 x.Status == InstanceStatus.Error) &&
                x.ModifiedAt != null &&
                x.ModifiedAt < threshold);

        var stuckList = stuck.ToList();
        if (stuckList.Count == 0)
        {
            _logger.LogInformation(
                "No stuck instances found (threshold: {Threshold:u}).", threshold);
            return 0;
        }

        _logger.LogWarning(
            "Found {Count} stuck instances older than {Threshold:u}. Terminating...",
            stuckList.Count, threshold);

        foreach (var instance in stuckList)
        {
            if (cancellationToken.IsCancellationRequested) break;

            _logger.LogInformation(
                "Terminating stuck instance {Id} ({Name}) in state {Status}, " +
                "last modified {ModifiedAt:u}.",
                instance.Id, instance.Name, instance.Status, instance.ModifiedAt);

            instance.Status = InstanceStatus.Terminated;
            await _repository.UpdateAsync(instance);
        }

        return stuckList.Count;
    }
}