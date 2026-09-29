using ComputeNexus.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputeNexus.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IExcelExportService _excelExportService;

    public ExportController(IExcelExportService excelExportService)
    {
        _excelExportService = excelExportService;
    }

    [HttpGet("billing/{userId}")]
    public async Task<IActionResult> ExportBilling(string userId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var bytes = await _excelExportService.ExportBillingReportAsync(userId, from, to);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"billing-{userId}-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}