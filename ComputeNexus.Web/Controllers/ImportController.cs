using ClosedXML.Excel;
using ComputeNexus.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComputeNexus.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly IExcelImportService _importService;

    public ImportController(IExcelImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("instances")]
    public async Task<IActionResult> ImportInstances(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded");

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx")
            return BadRequest("Only .xlsx files are supported");

        using var stream = file.OpenReadStream();
        var result = await _importService.ImportComputeInstancesAsync(stream);

        if (result.HasErrors)
        {
            return Ok(new
            {
                success = false,
                totalRows = result.TotalRows,
                successCount = result.SuccessfulRecords.Count,
                errorCount = result.Errors.Count,
                errors = result.Errors
            });
        }

        return Ok(new
        {
            success = true,
            totalRows = result.TotalRows,
            importedCount = result.SuccessfulRecords.Count
        });
    }

    [HttpGet("template")]
    public IActionResult GetImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Instance Import");

        var headers = new[] { "name", "gpu_count", "cpu_cores", "memory_gb", "storage_gb" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        // example row
        ws.Cell(2, 1).Value = "My Training Instance";
        ws.Cell(2, 2).Value = 2;
        ws.Cell(2, 3).Value = 8;
        ws.Cell(2, 4).Value = 32;
        ws.Cell(2, 5).Value = 100;

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "instance-import-template.xlsx");
    }
}