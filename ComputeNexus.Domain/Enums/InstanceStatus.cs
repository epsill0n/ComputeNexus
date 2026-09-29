namespace ComputeNexus.Domain.Enums;

public enum InstanceStatus
{
    Pending,    // Provisioning requested
    Starting,   // Hardware being allocated
    Running,    // active and billing
    Stopping,   // Shutting down
    Stopped,    // Powered off
    Error,      // Failed state
    Terminated  // Permanently deleted
}