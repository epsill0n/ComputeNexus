using ComputeNexus.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ComputeNexus.Repository;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<User>(options)
{
    public DbSet<ComputeInstance> ComputeInstances { get; set; }
    public DbSet<HardwareNode> HardwareNodes { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobLog> JobLogs { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // decimal configuration
        modelBuilder.Entity<ComputeInstance>(entity =>
        {
            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,4)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,4)");
            entity.Property(e => e.EstimatedMonthlyCost).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<HardwareNode>(entity =>
        {
            entity.Property(e => e.PricePerGpuHour).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.Property(e => e.ComputeCost).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreditBalance).HasColumnType("decimal(18,4)");
        });

        // relationship configuration

        // ComputeInstance -> User (Owner)
        modelBuilder.Entity<ComputeInstance>()
            .HasOne(e => e.User)
            .WithMany(u => u.ComputeInstances)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ComputeInstance -> HardwareNode
        modelBuilder.Entity<ComputeInstance>()
            .HasOne(e => e.HardwareNode)
            .WithMany(h => h.Instances)
            .HasForeignKey(e => e.HardwareNodeId)
            .OnDelete(DeleteBehavior.SetNull);

        // ComputeInstance -> Template
        modelBuilder.Entity<ComputeInstance>()
            .HasOne(e => e.Template)
            .WithMany(t => t.Instances)
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Job -> ComputeInstance
        modelBuilder.Entity<Job>()
            .HasOne(e => e.ComputeInstance)
            .WithMany(ci => ci.Jobs)
            .HasForeignKey(e => e.ComputeInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Job -> User (Owner)
        modelBuilder.Entity<Job>()
            .HasOne(e => e.User)
            .WithMany(u => u.Jobs)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment -> User (Owner)
        modelBuilder.Entity<Payment>()
            .HasOne(e => e.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Template -> User (Creator)
        modelBuilder.Entity<Template>()
            .HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ternary relationship: JobLog
        modelBuilder.Entity<JobLog>(entity =>
        {
            // JobLog -> Job
            entity.HasOne(jl => jl.Job)
                .WithMany(j => j.JobLogs)
                .HasForeignKey(jl => jl.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobLog -> User
            entity.HasOne(jl => jl.User)
                .WithMany(u => u.JobLogs)
                .HasForeignKey(jl => jl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobLog -> ComputeInstance
            entity.HasOne(jl => jl.ComputeInstance)
                .WithMany(ci => ci.JobLogs)
                .HasForeignKey(jl => jl.ComputeInstanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}