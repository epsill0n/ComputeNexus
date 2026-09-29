using ComputeNexus.Domain.Entities;
using ComputeNexus.Repository.Interface;
using EFCore.BulkExtensions;

namespace ComputeNexus.Repository.Implementation;

public class HardwareNodeRepository : Repository<HardwareNode>, IHardwareNodeRepository
{
    public HardwareNodeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task BulkInsertOrUpdateAsync(List<HardwareNode> nodes)
    {
        await _context.BulkInsertOrUpdateAsync(nodes);
    }
}