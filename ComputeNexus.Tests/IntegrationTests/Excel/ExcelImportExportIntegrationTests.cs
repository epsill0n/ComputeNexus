using ClosedXML.Excel;
using ComputeNexus.Tests.IntegrationTests.Fixtures;

namespace ComputeNexus.Tests.IntegrationTests.Excel;

public class ExcelImportExportIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public ExcelImportExportIntegrationTests(WebApplicationFactoryFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task ExportBilling_WithValidUserId_ReturnsExcelFile()
    {
        // act
        var userId = Guid.NewGuid().ToString();
        var response = await _client.GetAsync(
            $"/api/export/billing/{userId}?from=2026-01-01&to=2026-12-31");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetImportTemplate_ReturnsExcelFile()
    {
        // act
        var response = await _client.GetAsync("/api/import/template");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ImportInstances_WithValidExcel_ImportsSuccessfully()
    {
        // arrange - create excel file in memory
        using var stream = new MemoryStream();
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Instance Import");
    
        ws.Cell(1, 1).Value = "name";
        ws.Cell(1, 2).Value = "gpu_count";
        ws.Cell(1, 3).Value = "cpu_cores";
        ws.Cell(1, 4).Value = "memory_gb";
        ws.Cell(1, 5).Value = "storage_gb";
        ws.Cell(1, 6).Value = "user_id";
    
        ws.Cell(2, 1).Value = "Import Test 1";
        ws.Cell(2, 2).Value = 2;
        ws.Cell(2, 3).Value = 8;
        ws.Cell(2, 4).Value = 32;
        ws.Cell(2, 5).Value = 100;
        ws.Cell(2, 6).Value = "test-user-id-123";
    
        workbook.SaveAs(stream);
        stream.Position = 0;

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", "test.xlsx");

        // act
        var response = await _client.PostAsync("/api/import/instances", content);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ImportResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.ImportedCount.Should().Be(1);
    }

    private class ImportResponse
    {
        public bool Success { get; set; }
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
    }
}