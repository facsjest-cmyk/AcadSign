using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/admin/attestations")]
[Authorize(Roles = "Administrator,Registrar")]
public class AttestationsAdminController : ControllerBase
{
    private readonly IAttestationBatchGenerationService _service;

    public AttestationsAdminController(IAttestationBatchGenerationService service)
    {
        _service = service;
    }

    [HttpPost("generate-from-sis")]
    [ProducesResponseType(typeof(SisAttestationBatchGenerationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<SisAttestationBatchGenerationResult>> GenerateFromSis(
        [FromQuery] DocumentType documentType = DocumentType.AttestationScolarite,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GenerateFromSisAsync(documentType, cancellationToken);
        return Ok(result);
    }
}
