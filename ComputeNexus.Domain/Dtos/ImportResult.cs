namespace ComputeNexus.Domain.Dtos;

public class ImportResult<T>
{
    public List<T> SuccessfulRecords { get; set; } = new();
    public List<ImportError> Errors { get; set; } = new();
    public int TotalRows { get; set; }
    public bool HasErrors => Errors.Count > 0;
}
