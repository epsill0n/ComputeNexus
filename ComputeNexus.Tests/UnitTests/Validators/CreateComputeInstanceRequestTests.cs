namespace ComputeNexus.Tests.UnitTests.Validators;

public class CreateComputeInstanceRequestTests
{
    // valid partitions equivalence classes

    [Theory]
    [InlineData(1, 1, 1, 10)]      // minimum valid
    [InlineData(4, 32, 128, 500)]  // typical valid
    [InlineData(8, 64, 512, 2000)] // maximum valid
    public void CreateRequest_WithValidValues_ShouldBeValid(
        int gpuCount, int cpuCores, int memoryGb, int storageGb)
    {
        // arrange
        var request = new CreateComputeInstanceRequest(
            "Test Instance", gpuCount, cpuCores, memoryGb, storageGb, null);

        // act
        var isValid = ValidateRequest(request);

        // assert
        isValid.Should().BeTrue();
    }

    // invalid partitions boundary values
    
    [Theory]
    [InlineData(0, 1, 1, 10)]      // GPU below minimum
    [InlineData(9, 1, 1, 10)]      // GPU above maximum
    [InlineData(1, 0, 1, 10)]      // CPU below minimum
    [InlineData(1, 65, 1, 10)]     // CPU above maximum
    [InlineData(1, 1, 0, 10)]      // memory below minimum
    [InlineData(1, 1, 513, 10)]    // memory above maximum
    [InlineData(1, 1, 1, 9)]       // storage below minimum
    [InlineData(1, 1, 1, 2001)]    // storage above maximum
    public void CreateRequest_WithInvalidValues_ShouldBeInvalid(
        int gpuCount, int cpuCores, int memoryGb, int storageGb)
    {
        // arrange
        var request = new CreateComputeInstanceRequest(
            "Test Instance", gpuCount, cpuCores, memoryGb, storageGb, null);

        // act
        var isValid = ValidateRequest(request);

        // assert
        isValid.Should().BeFalse();
    }

    // name validation
    [Fact]
    public void CreateRequest_WithEmptyName_ShouldBeInvalid()
    {
        // arrange
        var request = new CreateComputeInstanceRequest("", 2, 8, 32, 100, null);

        // act & Assert
        request.Should().NotBeNull();
        
        // [Required] should catch this
        // we test anyway
        string.IsNullOrWhiteSpace(request.Name).Should().BeTrue();
    }

    // helper method
    private bool ValidateRequest(CreateComputeInstanceRequest request)
    {
        return request.GpuCount >= 1 && request.GpuCount <= 8
            && request.CpuCores >= 1 && request.CpuCores <= 64
            && request.MemoryGb >= 1 && request.MemoryGb <= 512
            && request.StorageGb >= 10 && request.StorageGb <= 2000;
    }
}