namespace ComputeNexus.Domain.Results;

public class SyncResult
{
    public bool Success { get; set; }
    public int NodesProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}