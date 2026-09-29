using Microsoft.AspNetCore.Identity;

namespace ComputeNexus.Domain.Entities;

public class User : IdentityUser
{
    // public string Email { get; set; } = string.Empty; // given from IdentityUser
    // public string Username { get; set; } = string.Empty; // given from IdentityUser
    public decimal CreditBalance { get; set; }
    
    public virtual ICollection<ComputeInstance> ComputeInstances { get; set; } = new List<ComputeInstance>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    public virtual ICollection<JobLog> JobLogs { get; set; } = new List<JobLog>();
}