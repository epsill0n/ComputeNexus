namespace ComputeNexus.Domain.Enums;

public enum JobStatus
{
    Queued,      // Waiting in the channel
    Preparing,   // Setting up environment
    Running,     // Training/inference executing
    Completed,   // Successfully finished
    Failed,      // Error occurred
    Cancelled    // Manually stopped
}