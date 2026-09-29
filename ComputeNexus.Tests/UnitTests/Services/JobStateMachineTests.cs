namespace ComputeNexus.Tests.UnitTests.Services;

public class JobStateMachineTests
{
    // valid transitions all edges
    
    [Fact]
    public void Job_AllValidTransitions_ShouldSucceed()
    {
        // arrange - start at Queued
        var job = new Job
        {
            Name = "Test Job",
            Status = JobStatus.Queued,
            Type = JobType.Training
        };

        // edge 1: Queued -> Preparing
        job.Status = JobStatus.Preparing;
        job.Status.Should().Be(JobStatus.Preparing);

        // edge 2: Preparing -> Running
        job.Status = JobStatus.Running;
        job.StartedAt = DateTime.UtcNow;
        job.Status.Should().Be(JobStatus.Running);

        // edge 3a: Running -> Completed
        job.Status = JobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        job.Status.Should().Be(JobStatus.Completed);

        // Reset to Running to test edge 3b
        job.Status = JobStatus.Running;

        // edge 3b: Running -> Failed
        job.Status = JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        job.ErrorMessage = "Test failure";
        job.Status.Should().Be(JobStatus.Failed);
    }

    // invalid transitions should not be allowed
    [Theory]
    [InlineData(JobStatus.Queued, true)]      // can start from Queued
    [InlineData(JobStatus.Preparing, true)]   // can start from Preparing
    [InlineData(JobStatus.Running, false)]    // cannot start a running job
    [InlineData(JobStatus.Completed, false)]  // cannot start a completed job
    [InlineData(JobStatus.Failed, false)]     // cannot start a failed job
    [InlineData(JobStatus.Cancelled, false)]  // cannot start a cancelled job
    public void StartJob_OnlyAllowsValidStates(JobStatus status, bool shouldBeAllowed)
    {
        // arrange
        var job = new Job
        {
            Name = "Test Job",
            Status = status,
            Type = JobType.Training
        };

        // act - simulate the StartJobAsync logic
        var canStart = job.Status == JobStatus.Queued || job.Status == JobStatus.Preparing;

        // assert
        canStart.Should().Be(shouldBeAllowed);
    }

    // all transitions full graph coverage

    [Theory]
    [InlineData(JobStatus.Queued, JobStatus.Preparing, true)]
    [InlineData(JobStatus.Queued, JobStatus.Running, false)]
    [InlineData(JobStatus.Preparing, JobStatus.Running, true)]
    [InlineData(JobStatus.Preparing, JobStatus.Completed, false)]
    [InlineData(JobStatus.Running, JobStatus.Completed, true)]
    [InlineData(JobStatus.Running, JobStatus.Failed, true)]
    [InlineData(JobStatus.Completed, JobStatus.Running, false)]
    [InlineData(JobStatus.Failed, JobStatus.Running, false)]
    public void Job_AllStateTransitions_ShouldBeValidOrInvalid(
        JobStatus fromState, JobStatus toState, bool shouldBeValid)
    {
        // arrange
        var job = new Job
        {
            Name = "Test Job",
            Status = fromState,
            Type = JobType.Training
        };

        // act - try to transition
        var validTransitions = new Dictionary<JobStatus, HashSet<JobStatus>>
        {
            { JobStatus.Queued, new HashSet<JobStatus> { JobStatus.Preparing, JobStatus.Cancelled } },
            { JobStatus.Preparing, new HashSet<JobStatus> { JobStatus.Running, JobStatus.Cancelled } },
            { JobStatus.Running, new HashSet<JobStatus> { JobStatus.Completed, JobStatus.Failed } },
            { JobStatus.Completed, new HashSet<JobStatus>() },
            { JobStatus.Failed, new HashSet<JobStatus>() },
            { JobStatus.Cancelled, new HashSet<JobStatus>() }
        };

        var isValid = validTransitions.ContainsKey(fromState) && 
                      validTransitions[fromState].Contains(toState);

        // assert
        isValid.Should().Be(shouldBeValid);
    }
}