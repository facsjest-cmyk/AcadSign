using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Enums;

namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IAttestationTemplateRenderer
{
    Task<byte[]?> TryRenderAsync(
        DocumentType type,
        StudentData data,
        CancellationToken cancellationToken = default);
}
