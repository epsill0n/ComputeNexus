using ComputeNexus.Domain.Entities;

namespace ComputeNexus.Repository.Interface;

public interface IHardwareNodeRepository
{
    Task BulkInsertOrUpdateAsync(List<HardwareNode> nodes);
}