namespace ComputeNexus.Domain.Dtos.Email;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainText { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
}