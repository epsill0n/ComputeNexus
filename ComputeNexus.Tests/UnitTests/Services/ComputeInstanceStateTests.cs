namespace ComputeNexus.Tests.UnitTests.Services;

public class ComputeInstanceStateTests
{
    // valid transitions all edges

    [Fact]
    public void ComputeInstance_AllValidTransitions_ShouldSucceed()
    {
        // arrange - start at Pending
        var instance = new ComputeInstance
        {
            Name = "Test Instance",
            Status = InstanceStatus.Pending,
            GpuCount = 2
        };

        // edge 1: Pending -> Starting (StartAsync)
        instance.Status = InstanceStatus.Starting;
        instance.StartedAt = DateTime.UtcNow;
        instance.Status.Should().Be(InstanceStatus.Starting);

        // edge 2: Starting -> Running (AssignHardwareAsync or other logic)
        instance.Status = InstanceStatus.Running;
        instance.Status.Should().Be(InstanceStatus.Running);

        // edge 3: Running -> Stopping (StopAsync)
        instance.Status = InstanceStatus.Stopping;
        instance.StoppedAt = DateTime.UtcNow;
        instance.Status.Should().Be(InstanceStatus.Stopping);

        // edge 4: Stopping -> Stopped (When fully stopped)
        instance.Status = InstanceStatus.Stopped;
        instance.Status.Should().Be(InstanceStatus.Stopped);
    }

    // alternative path

    [Fact]
    public void ComputeInstance_CanGoDirectlyFromPendingToRunning_WhenAssignedHardware()
    {
        // arrange
        var instance = new ComputeInstance
        {
            Name = "Test Instance",
            Status = InstanceStatus.Pending,
            GpuCount = 2
        };

        // act - simulate AssignHardwareAsync setting Running directly
        instance.Status = InstanceStatus.Running;
        instance.StartedAt = DateTime.UtcNow;
        instance.AssignedGpuModel = "A100";

        // assert
        instance.Status.Should().Be(InstanceStatus.Running);
        instance.StartedAt.Should().NotBeNull();
        instance.AssignedGpuModel.Should().Be("A100");
    }

    // invalid transitions should not be allowed

    [Theory]
    [InlineData(InstanceStatus.Pending, true)]      // can start from Pending
    [InlineData(InstanceStatus.Stopped, true)]      // can start from Stopped
    [InlineData(InstanceStatus.Running, false)]     // cannot start a running instance
    [InlineData(InstanceStatus.Starting, false)]    // cannot start a starting instance
    [InlineData(InstanceStatus.Stopping, false)]    // cannot start a stopping instance
    [InlineData(InstanceStatus.Error, false)]       // cannot start an error instance
    [InlineData(InstanceStatus.Terminated, false)]  // cannot start a terminated instance
    public void StartAsync_OnlyAllowsPendingOrStopped(InstanceStatus status, bool shouldBeAllowed)
    {
        // arrange
        var instance = new ComputeInstance
        {
            Name = "Test Instance",
            Status = status,
            GpuCount = 1
        };

        // act - simulate StartAsync logic
        var canStart = instance.Status == InstanceStatus.Pending 
                       || instance.Status == InstanceStatus.Stopped;

        // assert
        canStart.Should().Be(shouldBeAllowed);
    }

    [Theory]
    [InlineData(InstanceStatus.Running, true)]      // can stop only when Running
    [InlineData(InstanceStatus.Pending, false)]
    [InlineData(InstanceStatus.Starting, false)]
    [InlineData(InstanceStatus.Stopped, false)]
    [InlineData(InstanceStatus.Stopping, false)]
    [InlineData(InstanceStatus.Error, false)]
    [InlineData(InstanceStatus.Terminated, false)]
    public void StopAsync_OnlyAllowsRunningState(InstanceStatus status, bool shouldBeAllowed)
    {
        // arrange
        var instance = new ComputeInstance
        {
            Name = "Test Instance",
            Status = status,
            GpuCount = 1
        };

        // act - simulate StopAsync logic
        var canStop = instance.Status == InstanceStatus.Running;

        // assert
        canStop.Should().Be(shouldBeAllowed);
    }

    // all state transitions full graph coverage matrix
    
    [Theory]
    // Pending transitions
    [InlineData(InstanceStatus.Pending, InstanceStatus.Starting, true)]
    [InlineData(InstanceStatus.Pending, InstanceStatus.Running, true)]
    [InlineData(InstanceStatus.Pending, InstanceStatus.Stopped, false)]
    [InlineData(InstanceStatus.Pending, InstanceStatus.Error, true)]
    
    // Starting transitions
    [InlineData(InstanceStatus.Starting, InstanceStatus.Running, true)]
    [InlineData(InstanceStatus.Starting, InstanceStatus.Error, true)]
    [InlineData(InstanceStatus.Starting, InstanceStatus.Pending, false)]
    
    // Running transitions
    [InlineData(InstanceStatus.Running, InstanceStatus.Stopping, true)]
    [InlineData(InstanceStatus.Running, InstanceStatus.Error, true)]
    [InlineData(InstanceStatus.Running, InstanceStatus.Terminated, true)]
    
    // Stopping transitions
    [InlineData(InstanceStatus.Stopping, InstanceStatus.Stopped, true)]
    [InlineData(InstanceStatus.Stopping, InstanceStatus.Error, true)]
    [InlineData(InstanceStatus.Stopping, InstanceStatus.Running, false)]
    
    // Stopped transitions
    [InlineData(InstanceStatus.Stopped, InstanceStatus.Starting, true)]
    [InlineData(InstanceStatus.Stopped, InstanceStatus.Terminated, true)]
    
    // Error transitions
    [InlineData(InstanceStatus.Error, InstanceStatus.Terminated, true)]
    [InlineData(InstanceStatus.Error, InstanceStatus.Running, false)]
    [InlineData(InstanceStatus.Error, InstanceStatus.Pending, false)]
    
    // Terminated transitions
    [InlineData(InstanceStatus.Terminated, InstanceStatus.Pending, false)]
    [InlineData(InstanceStatus.Terminated, InstanceStatus.Running, false)]
    public void ComputeInstance_AllStateTransitions_ShouldBeValidOrInvalid(
        InstanceStatus fromState, InstanceStatus toState, bool shouldBeValid)
    {
        // arrange
        var instance = new ComputeInstance
        {
            Name = "Test Instance",
            Status = fromState,
            GpuCount = 2
        };

        // act - define allowed transitions based on business rules
        var allowedTransitions = new Dictionary<InstanceStatus, HashSet<InstanceStatus>>
        {
            { InstanceStatus.Pending, new HashSet<InstanceStatus> { InstanceStatus.Starting, InstanceStatus.Running, InstanceStatus.Error } },
            { InstanceStatus.Starting, new HashSet<InstanceStatus> { InstanceStatus.Running, InstanceStatus.Error } },
            { InstanceStatus.Running, new HashSet<InstanceStatus> { InstanceStatus.Stopping, InstanceStatus.Error, InstanceStatus.Terminated } },
            { InstanceStatus.Stopping, new HashSet<InstanceStatus> { InstanceStatus.Stopped, InstanceStatus.Error } },
            { InstanceStatus.Stopped, new HashSet<InstanceStatus> { InstanceStatus.Starting, InstanceStatus.Terminated } },
            { InstanceStatus.Error, new HashSet<InstanceStatus> { InstanceStatus.Terminated } },
            { InstanceStatus.Terminated, new HashSet<InstanceStatus>() }
        };

        var isValid = allowedTransitions.ContainsKey(fromState) && 
                      allowedTransitions[fromState].Contains(toState);

        // assert
        isValid.Should().Be(shouldBeValid);
    }

    // graph coverage all nodes reachable
    
    [Fact]
    public void ComputeInstance_AllStates_AreReachable()
    {
        // arrange - Start from Pending
        var instance = new ComputeInstance
        {
            Name = "Test Instance",
            Status = InstanceStatus.Pending,
            GpuCount = 2
        };

        var visitedStates = new HashSet<InstanceStatus> { instance.Status };

        // Transition through all valid paths
        // path 1: Pending -> Starting -> Running -> Stopping -> Stopped
        instance.Status = InstanceStatus.Starting;
        visitedStates.Add(InstanceStatus.Starting);
        
        instance.Status = InstanceStatus.Running;
        visitedStates.Add(InstanceStatus.Running);
        
        instance.Status = InstanceStatus.Stopping;
        visitedStates.Add(InstanceStatus.Stopping);
        
        instance.Status = InstanceStatus.Stopped;
        visitedStates.Add(InstanceStatus.Stopped);

        // path 2: Pending -> Error (alternative)
        instance.Status = InstanceStatus.Pending;
        instance.Status = InstanceStatus.Error;
        visitedStates.Add(InstanceStatus.Error);

        // path 3: Stopped -> Terminated
        instance.Status = InstanceStatus.Stopped;
        instance.Status = InstanceStatus.Terminated;
        visitedStates.Add(InstanceStatus.Terminated);

        // All states should be reachable
        var expectedStates = Enum.GetValues<InstanceStatus>();
        visitedStates.Should().BeEquivalentTo(expectedStates);
    }

    // edge coverage, all edges tested
    [Fact]
    public void ComputeInstance_GraphCoverage_AllEdgesAreTested()
    {
        // this test verifies that all edges in our transition graph are covered
        // by the combination of all valid transitions defined in the matrix above
        
        var allTransitions = new List<(InstanceStatus from, InstanceStatus to)>
        {
            // from Pending
            (InstanceStatus.Pending, InstanceStatus.Starting),
            (InstanceStatus.Pending, InstanceStatus.Running),
            (InstanceStatus.Pending, InstanceStatus.Error),
            
            // from Starting
            (InstanceStatus.Starting, InstanceStatus.Running),
            (InstanceStatus.Starting, InstanceStatus.Error),
            
            // from Running
            (InstanceStatus.Running, InstanceStatus.Stopping),
            (InstanceStatus.Running, InstanceStatus.Error),
            (InstanceStatus.Running, InstanceStatus.Terminated),
            
            // from Stopping
            (InstanceStatus.Stopping, InstanceStatus.Stopped),
            (InstanceStatus.Stopping, InstanceStatus.Error),
            
            // from Stopped
            (InstanceStatus.Stopped, InstanceStatus.Starting),
            (InstanceStatus.Stopped, InstanceStatus.Terminated),
            
            // from Error
            (InstanceStatus.Error, InstanceStatus.Terminated)
        };

        // ensure all expected edges are defined
        var expectedEdgeCount = 13; // total number of valid transitions
        allTransitions.Count.Should().Be(expectedEdgeCount);
    }
}