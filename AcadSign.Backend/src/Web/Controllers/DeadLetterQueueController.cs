using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcadSign.Backend.Application.Services;
using System.Security.Claims;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/admin/dead-letter-queue")]
[Authorize(Roles = "Admin")]
public class DeadLetterQueueController : ControllerBase
{
    private readonly DeadLetterQueueService _dlqService;
    private readonly ILogger<DeadLetterQueueController> _logger;
    
    public DeadLetterQueueController(
        DeadLetterQueueService dlqService,
        ILogger<DeadLetterQueueController> logger)
    {
        _dlqService = dlqService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUnresolvedEntries()
    {
        var entries = await _dlqService.GetUnresolvedEntriesAsync();
        
        return Ok(new
        {
            total = entries.Count,
            entries = entries.Select(e => new
            {
                id = e.Id,
                documentId = e.DocumentId,
                errorMessage = e.ErrorMessage,
                retryCount = e.RetryCount,
                createdAt = e.CreatedAt,
                lastRetryAt = e.LastRetryAt
            })
        });
    }
    
    [HttpPost("{dlqId}/retry")]
    public async Task<IActionResult> RetryEntry(Guid dlqId)
    {
        var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        
        try
        {
            await _dlqService.RetryFromDeadLetterQueueAsync(dlqId, adminUserId);
            
            return Ok(new { message = "Entry retried successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
