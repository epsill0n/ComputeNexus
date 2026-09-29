using System.Reflection;
using ComputeNexus.Service.Implementation;
using Microsoft.Extensions.Logging.Abstractions;

namespace ComputeNexus.Tests.UnitTests.Services;

public class PricingCalculatorTests
{
    private readonly MethodInfo _calculateMethod;
    private readonly object _serviceInstance;

    public PricingCalculatorTests()
    {
        var serviceType = typeof(ComputeInstanceService);
        _calculateMethod = serviceType.GetMethod("CalculateHourlyRate", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        
        _serviceInstance = Activator.CreateInstance(serviceType, 
            new object?[] { null, null, NullLogger<ComputeInstanceService>.Instance })!;
    }


    [Theory]
    [InlineData(1, 1, 1, 1.512)]      // 0.50 + 1.00 + 0.01 + 0.002 = 1.512
    [InlineData(2, 8, 32, 2.644)]     // 0.50 + 2.00 + 0.08 + 0.064 = 2.644
    [InlineData(4, 16, 64, 4.788)]    // 0.50 + 4.00 + 0.16 + 0.128 = 4.788
    [InlineData(8, 32, 128, 9.076)]   // 0.50 + 8.00 + 0.32 + 0.256 = 9.076
    public void CalculateHourlyRate_WithVariousInputs_ReturnsCorrectPrice(
        int gpuCount, int cpuCores, int memoryGb, double expected)
    {
        // act
        var result = _calculateMethod.Invoke(_serviceInstance, 
            new object[] { gpuCount, cpuCores, memoryGb });

        ((decimal)result).Should().BeApproximately((decimal)expected, 0.0001m);
    }

    // boundary value tests
    [Theory]
    [InlineData(0, 0, 0, 0.50)]       // only base rate
    [InlineData(1, 0, 0, 1.50)]       // only GPU cost
    [InlineData(0, 1, 0, 0.51)]       // only CPU cost
    [InlineData(0, 0, 1, 0.502)]      // only memory cost
    public void CalculateHourlyRate_WithZeroValues_ReturnsCorrectPartialPrice(
        int gpuCount, int cpuCores, int memoryGb, double expected)
    {
        // act
        var result = _calculateMethod.Invoke(_serviceInstance, 
            new object[] { gpuCount, cpuCores, memoryGb });

        // assert
        ((decimal)result).Should().BeApproximately((decimal)expected, 0.0001m);
    }

    // computational complexity tests
    [Fact]
    public void CalculateHourlyRate_WithLargeValues_ReturnsReasonablePrice()
    {
        // arrange
        const int gpuCount = 8;
        const int cpuCores = 64;
        const int memoryGb = 512;

        // act
        var result = _calculateMethod.Invoke(_serviceInstance, 
            new object[] { gpuCount, cpuCores, memoryGb });

        // assert
        var expected = 0.50m + 8 * 1.00m + 64 * 0.01m + 512 * 0.002m;
        result.Should().Be(expected);
        ((decimal)result).Should().BeGreaterThan(0);
    }

    // mutation detection base rate constant

    [Fact]
    public void CalculateHourlyRate_BaseRateShouldAlwaysBeIncluded()
    {
        // arrange - all zero counts should still include base rate
        var expected = 0.50m;

        // act
        var result = _calculateMethod.Invoke(_serviceInstance, 
            new object[] { 0, 0, 0 });

        // assert
        result.Should().Be(expected);
    }
}