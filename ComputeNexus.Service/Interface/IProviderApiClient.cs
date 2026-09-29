using System.Collections.Generic;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos;

namespace ComputeNexus.Service.Interface;

public interface IProviderApiClient
{
    Task<List<ProviderHardwareDto>> GetAvailableHardwareAsync();
}