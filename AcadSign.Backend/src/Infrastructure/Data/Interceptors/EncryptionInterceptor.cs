using System.Reflection;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AcadSign.Backend.Infrastructure.Data.Interceptors;

public class EncryptionInterceptor : SaveChangesInterceptor
{
    private readonly IPiiEncryptionService _encryptionService;

    public EncryptionInterceptor(IPiiEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        EncryptProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        EncryptProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EncryptProperties(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var encryptedProperties = entry.Entity.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttribute<EncryptedPropertyAttribute>() != null);

            foreach (var property in encryptedProperties)
            {
                var value = property.GetValue(entry.Entity) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    // Check if already encrypted (to avoid double encryption on update)
                    var currentValue = entry.Property(property.Name).CurrentValue as string;
                    var originalValue = entry.Property(property.Name).OriginalValue as string;
                    
                    // Only encrypt if the value has changed or it's a new entity
                    if (entry.State == EntityState.Added || currentValue != originalValue)
                    {
                        var encryptedValue = _encryptionService.Encrypt(value);
                        property.SetValue(entry.Entity, encryptedValue);
                    }
                }
            }
        }
    }
}
