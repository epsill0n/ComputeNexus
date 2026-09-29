using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Entities;
using ComputeNexus.Domain.Enums;
using ComputeNexus.Repository.Interface;
using ComputeNexus.Service.Interface;
using Microsoft.AspNetCore.Identity;

namespace ComputeNexus.Service.Implementation;

public class ExcelImportService : IExcelImportService
{
    private readonly IRepository<ComputeInstance> _instanceRepository;
    private readonly IRepository<Template> _templateRepository;
    private readonly UserManager<User> _userManager;

    public ExcelImportService(
        IRepository<ComputeInstance> instanceRepository,
        IRepository<Template> templateRepository,
        UserManager<User> userManager)
    {
        _instanceRepository = instanceRepository;
        _templateRepository = templateRepository;
        _userManager = userManager;
    }

    public async Task<ImportResult<ComputeInstanceImportDto>> ImportComputeInstancesAsync(Stream fileStream)
    {
        var result = new ImportResult<ComputeInstanceImportDto>();

        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1);


        var expectedHeaders = new Dictionary<string, int>();
        var headerRow = ws.Row(1);
        for (int col = 1; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
        {
            expectedHeaders[headerRow.Cell(col).GetString().Trim().ToLower()] = col;
        }


        var requiredHeaders = new[] { "name", "gpu_count", "cpu_cores", "memory_gb", "storage_gb" };
        foreach (var h in requiredHeaders)
        {
            if (!expectedHeaders.ContainsKey(h))
            {
                result.Errors.Add(new ImportError
                {
                    Row = 1,
                    Column = h,
                    Message = $"Missing required column: '{h}'"
                });
            }
        }

        if (result.HasErrors) return result;

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        result.TotalRows = lastRow - 1;


        var allTemplates = await _templateRepository.GetAllAsync(
            selector: x => x,
            predicate: x => x.IsPublic
        );
        var templateDict = allTemplates.ToDictionary(t => t.Id);


        for (int row = 2; row <= lastRow; row++)
        {
            var name = ws.Cell(row, expectedHeaders["name"]).GetString().Trim();
            if (string.IsNullOrEmpty(name))
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "Name", Message = "Name is required" });
                continue;
            }

            if (!ws.Cell(row, expectedHeaders["gpu_count"]).TryGetValue(out int gpuCount) || gpuCount < 1 || gpuCount > 8)
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "GPU Count", Message = "GPU count must be between 1 and 8" });
                continue;
            }

            if (!ws.Cell(row, expectedHeaders["cpu_cores"]).TryGetValue(out int cpuCores) || cpuCores < 1 || cpuCores > 64)
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "CPU Cores", Message = "CPU cores must be between 1 and 64" });
                continue;
            }

            if (!ws.Cell(row, expectedHeaders["memory_gb"]).TryGetValue(out int memoryGb) || memoryGb < 1 || memoryGb > 512)
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "Memory GB", Message = "Memory must be between 1 and 512 GB" });
                continue;
            }

            if (!ws.Cell(row, expectedHeaders["storage_gb"]).TryGetValue(out int storageGb) || storageGb < 10 || storageGb > 2000)
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "Storage GB", Message = "Storage must be between 10 and 2000 GB" });
                continue;
            }


            Guid? templateId = null;
            if (expectedHeaders.TryGetValue("template_id", out var templateCol))
            {
                if (ws.Cell(row, templateCol).TryGetValue(out Guid parsedTemplateId))
                {
                    if (templateDict.ContainsKey(parsedTemplateId))
                    {
                        templateId = parsedTemplateId;
                    }
                    else
                    {
                        result.Errors.Add(new ImportError
                            { Row = row, Column = "Template ID", Message = $"Template '{parsedTemplateId}' not found" });
                        continue;
                    }
                }
            }


            string userId = string.Empty;
            if (expectedHeaders.TryGetValue("user_id", out var userCol))
            {
                var userIdValue = ws.Cell(row, userCol).GetString().Trim();
                if (!string.IsNullOrEmpty(userIdValue))
                {
                    var user = await _userManager.FindByIdAsync(userIdValue);
                    if (user != null)
                    {
                        userId = user.Id;
                    }
                    else
                    {
                        result.Errors.Add(new ImportError
                            { Row = row, Column = "User ID", Message = $"User '{userIdValue}' not found" });
                        continue;
                    }
                }
            }

            var existingInstance = await _instanceRepository.GetAsync(
                selector: x => x,
                predicate: x => x.Name == name && x.UserId == userId
            );
            if (existingInstance != null && !string.IsNullOrEmpty(userId))
            {
                result.Errors.Add(new ImportError
                    { Row = row, Column = "Name", Message = $"Instance '{name}' already exists for this user" });
                continue;
            }

            result.SuccessfulRecords.Add(new ComputeInstanceImportDto
            {
                Name = name,
                GpuCount = gpuCount,
                CpuCores = cpuCores,
                MemoryGb = memoryGb,
                StorageGb = storageGb,
                TemplateId = templateId,
                UserId = userId != string.Empty ? userId.ToString() : null
            });
        }

        if (result.SuccessfulRecords.Any())
        {
            var instancesToInsert = new List<ComputeInstance>();

            foreach (var dto in result.SuccessfulRecords)
            {
                var instance = new ComputeInstance
                {
                    Name = dto.Name,
                    Status = InstanceStatus.Pending,
                    GpuCount = dto.GpuCount,
                    CpuCores = dto.CpuCores,
                    MemoryGb = dto.MemoryGb,
                    StorageGb = dto.StorageGb,
                    TemplateId = dto.TemplateId,
                    UserId = dto.UserId ?? string.Empty,
                    HourlyRate = CalculateHourlyRate(dto.GpuCount, dto.CpuCores, dto.MemoryGb),
                    TotalCost = 0,
                    CreatedAt = DateTime.UtcNow
                };
                instancesToInsert.Add(instance);
            }

            await _instanceRepository.InsertManyAsync(instancesToInsert);
        }

        return result;
    }

    private decimal CalculateHourlyRate(int gpuCount, int cpuCores, int memoryGb)
    {
        decimal baseRate = 0.50m;
        decimal gpuRate = gpuCount * 1.00m;
        decimal cpuRate = cpuCores * 0.01m;
        decimal memoryRate = memoryGb * 0.002m;
        
        return baseRate + gpuRate + cpuRate + memoryRate;
    }
}