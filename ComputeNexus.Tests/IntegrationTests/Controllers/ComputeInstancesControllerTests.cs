using ComputeNexus.Tests.IntegrationTests.Fixtures;

namespace ComputeNexus.Tests.IntegrationTests.Controllers;

public class ComputeInstancesControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public ComputeInstancesControllerTests(WebApplicationFactoryFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task CreateInstance_WithValidData_Returns201()
    {
        // arrange
        var request = new CreateComputeInstanceRequest(
            "Test GPU Instance", 2, 8, 32, 100, null);

        // act
        var response = await _client.PostAsJsonAsync("/api/computeinstances", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var instance = await response.Content.ReadFromJsonAsync<ComputeInstanceResponse>();
        instance.Should().NotBeNull();
        instance!.Name.Should().Be("Test GPU Instance");
        instance.GpuCount.Should().Be(2);
        instance.CpuCores.Should().Be(8);
    }

    [Fact]
    public async Task GetPaged_WithValidRequest_ReturnsPaginatedResponse()
    {
        // act
        var response = await _client.GetAsync(
            "/api/computeinstances/paged?pageNumber=0&pageSize=10");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<ComputeInstanceResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateInstance_WithInvalidGpuCount_ReturnsBadRequest()
    {
        // arrange - GPU count 0 is invalid
        var request = new CreateComputeInstanceRequest(
            "Invalid Instance", 0, 8, 32, 100, null);

        // act
        var response = await _client.PostAsJsonAsync("/api/computeinstances", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}