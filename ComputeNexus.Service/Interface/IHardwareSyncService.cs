using System.Threading.Tasks;
using ComputeNexus.Domain.Results;

namespace ComputeNexus.Service.Interface;

public interface IHardwareSyncService
{
    Task<SyncResult> SyncAllAsync();
}