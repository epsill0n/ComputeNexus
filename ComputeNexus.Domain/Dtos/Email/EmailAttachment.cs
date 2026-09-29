namespace ComputeNexus.Domain.Dtos.Email;

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string? ContentType { get; set; }
}