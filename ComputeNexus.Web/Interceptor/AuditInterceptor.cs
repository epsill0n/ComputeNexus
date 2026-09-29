using ComputeNexus.Domain.Entities;
using ComputeNexus.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ComputeNexus.Web.Interceptor;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context!;
        var entries = context.ChangeTracker
            .Entries<BaseAuditableEntity>();

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            var user = _currentUser.GetUserId() ?? "system";

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedById = user;
                entry.Entity.CreatedAt = now;
                entry.Entity.ModifiedById = user;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedById = user;
                entry.Entity.ModifiedAt = now;
            }
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context!;
        var entries = context.ChangeTracker
            .Entries<BaseAuditableEntity>();

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            var user = _currentUser.GetUserId() ?? "system";

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedById = user;
                entry.Entity.CreatedAt = now;
                entry.Entity.ModifiedById = user;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedById = user;
                entry.Entity.ModifiedAt = now;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}