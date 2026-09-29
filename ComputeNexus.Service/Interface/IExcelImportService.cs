using System.IO;
using System.Threading.Tasks;
using ComputeNexus.Domain.Dtos;

namespace ComputeNexus.Service.Interface;

public interface IExcelImportService
{
    Task<ImportResult<ComputeInstanceImportDto>> ImportComputeInstancesAsync(Stream fileStream);
}