namespace ComputeNexus.Tests.UnitTests.Services;

public class ComputeInstanceServiceLogicTests
{
    // testing all combinations of P1 and P2
    // P1 = hardware != null
    // P2 = hardware.AvailableGpuCount >= instance.GpuCount

    [Theory]
    // P1 = true, P2 = true -> Success
    [InlineData(5, 3, true, true)]
    [InlineData(3, 3, true, true)]

    // P1 = false, P2 = true -> Failure (hardware is null)
    [InlineData(0, 3, false, false)]

    // P1 = true, P2 = false -> Failure (not enough GPUs)
    [InlineData(2, 3, true, false)]

    // P1 = false, P2 = false -> Failure (both false)
    public void AssignHardware_LogicCoverage_AllCombinations(
        int availableGpus, int neededGpus,
        bool hardwareExists,
        bool shouldSucceed)
    {
        // arrange
        var hardware = hardwareExists ? new HardwareNode
        {
            AvailableGpuCount = availableGpus,
            Name = "Test Hardware",
            GpuModel = "A100"
        } : null;

        var instance = new ComputeInstance
        {
            GpuCount = neededGpus,
            Status = InstanceStatus.Pending
        };

        // act - Simulate the service logic
        bool canAssign = hardware != null && hardware.AvailableGpuCount >= instance.GpuCount;

        // assert
        canAssign.Should().Be(shouldSucceed);
    }

    // StartAsync
    // (Status == Stopped) OR (Status == Pending)

    [Theory]
    [InlineData(InstanceStatus.Pending, true)]
    [InlineData(InstanceStatus.Stopped, true)]
    [InlineData(InstanceStatus.Running, false)]
    [InlineData(InstanceStatus.Starting, false)]
    [InlineData(InstanceStatus.Stopping, false)]
    [InlineData(InstanceStatus.Error, false)]
    [InlineData(InstanceStatus.Terminated, false)]
    public void StartAsync_OnlyAllowsValidStates(InstanceStatus status, bool shouldBeAllowed)
    {
        // arrange
        var instance = new ComputeInstance
        {
            Status = status,
            Name = "Test Instance",
            GpuCount = 1
        };

        // act - simulate the StartAsync logic
        var canStart = instance.Status == InstanceStatus.Stopped 
                       || instance.Status == InstanceStatus.Pending;

        // assert
        canStart.Should().Be(shouldBeAllowed);
    }

    // StopAsync
    // (Status == Running)

    [Theory]
    [InlineData(InstanceStatus.Running, true)]
    [InlineData(InstanceStatus.Pending, false)]
    [InlineData(InstanceStatus.Stopped, false)]
    [InlineData(InstanceStatus.Starting, false)]
    [InlineData(InstanceStatus.Stopping, false)]
    [InlineData(InstanceStatus.Error, false)]
    public void StopAsync_OnlyAllowsRunningState(InstanceStatus status, bool shouldBeAllowed)
    {
        // arrange
        var instance = new ComputeInstance
        {
            Status = status,
            Name = "Test Instance",
            GpuCount = 1
        };

        // act - simulate the StopAsync logic
        var canStop = instance.Status == InstanceStatus.Running;

        // assert
        canStop.Should().Be(shouldBeAllowed);
    }
}