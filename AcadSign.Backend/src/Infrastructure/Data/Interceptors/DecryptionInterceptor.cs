using System.Reflection;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Attributes;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AcadSign.Backend.Infrastructure.Data.Interceptors;

public class DecryptionInterceptor : IMaterializationInterceptor
{
    private readonly IPiiEncryptionService _encryptionService;

    public DecryptionInterceptor(IPiiEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public object InitializedInstance(MaterializationInterceptionData materializationData, object entity)
    {
        DecryptProperties(entity);
        return entity;
    }

    public ValueTask<object> InitializedInstanceAsync(MaterializationInterceptionData materializationData, object entity, CancellationToken cancellationToken = default)
    {
        DecryptProperties(entity);
        return ValueTask.FromResult(entity);
    }

    private void DecryptProperties(object entity)
    {
        var encryptedProperties = entity.GetType()
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Where(p => p.GetCustomAttribute<EncryptedPropertyAttribute>() != null);

        foreach (var property in encryptedProperties)
        {
            if (property.GetValue(entity) is not string value || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            try
            {
                var decrypted = _encryptionService.Decrypt(value);
                property.SetValue(entity, decrypted);
            }
            catch
            {
                // Ignore values that cannot be decrypted (already plain or from a previous key).
            }
        }
    }
}
