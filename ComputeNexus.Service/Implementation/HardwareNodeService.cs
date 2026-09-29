using System.Linq.Expressions;
using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Repository.Interface;
using ComputeNexus.Service.Interface;

namespace ComputeNexus.Service.Implementation;

public class HardwareNodeService : IHardwareNodeService
{
    private readonly IRepository<HardwareNode> _repository;

    public HardwareNodeService(IRepository<HardwareNode> repository)
    {
        _repository = repository;
    }

    public async Task<List<HardwareNode>> GetAllAsync()
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            orderBy: q => q.OrderBy(h => h.Provider)
        );
        
        return result.ToList();
    }

    public async Task<HardwareNode?> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id
        );
    }

    public async Task<List<HardwareNode>> GetByProviderAsync(string provider)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: x => x.Provider.Contains(provider) || x.Provider == provider,
            orderBy: q => q.OrderBy(h => h.GpuModel)
        );
        return result.ToList();
    }

    public async Task<List<HardwareNode>> GetByGpuModelAsync(string gpuModel)
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: x => x.GpuModel.Contains(gpuModel),
            orderBy: q => q.OrderBy(h => h.PricePerGpuHour)
        );
        return result.ToList();
    }

    public async Task<List<HardwareNode>> GetAvailableAsync()
    {
        var result = await _repository.GetAllAsync(
            selector: x => x,
            predicate: x => x.AvailableGpuCount > 0 && x.IsActive,
            orderBy: q => q.OrderBy(h => h.PricePerGpuHour)
        );
        return result.ToList();
    }
    
    public async Task<PaginatedResult<HardwareNode>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? filter = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        Expression<Func<HardwareNode, bool>>? predicate = null;
        if (!string.IsNullOrEmpty(filter))
        {
            predicate = x => x.Name.Contains(filter) ||
                             x.Provider.Contains(filter) ||
                             x.GpuModel.Contains(filter) ||
                             (x.Region != null && x.Region.Contains(filter));
        }

        Func<IQueryable<HardwareNode>, IOrderedQueryable<HardwareNode>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = sortBy.ToLower() switch
            {
                "name" => sortDescending 
                    ? q => q.OrderByDescending(x => x.Name) 
                    : q => q.OrderBy(x => x.Name),
                "gpu" or "gpumodel" => sortDescending 
                    ? q => q.OrderByDescending(x => x.GpuModel) 
                    : q => q.OrderBy(x => x.GpuModel),
                "price" or "pricepergpu" => sortDescending 
                    ? q => q.OrderByDescending(x => x.PricePerGpuHour) 
                    : q => q.OrderBy(x => x.PricePerGpuHour),
                "available" or "availablegpus" => sortDescending 
                    ? q => q.OrderByDescending(x => x.AvailableGpuCount) 
                    : q => q.OrderBy(x => x.AvailableGpuCount),
                "region" => sortDescending 
                    ? q => q.OrderByDescending(x => x.Region) 
                    : q => q.OrderBy(x => x.Region),
                "provider" => sortDescending
                    ? q => q.OrderByDescending(x => x.Provider) 
                    : q => q.OrderBy(x => x.Provider),
                _ => sortDescending 
                    ? q => q.OrderByDescending(x => x.Name)
                    : q => q.OrderBy(x => x.Name)
            };
        }

        return await _repository.GetAllPagedAsync(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: predicate,
            orderBy: orderBy,
            asNoTracking: true
        );
    }
}