using ComputeNexus.Domain.Enums;

namespace ComputeNexus.Domain.Entities;

public class Payment : BaseAuditableEntity
{
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    public string UserId { get; set; }
    public virtual User User { get; set; } = null!;
}