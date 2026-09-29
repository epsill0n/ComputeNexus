using System;
using System.Threading.Tasks;

namespace ComputeNexus.Service.Interface;

public interface IExcelExportService
{
    Task<byte[]> ExportBillingReportAsync(string userId, DateTime from, DateTime to);
}