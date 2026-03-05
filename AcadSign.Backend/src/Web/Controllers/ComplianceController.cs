using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcadSign.Backend.Application.Services;
using System.Security.Claims;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/compliance")]
[Authorize(Roles = "Admin")]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceReportService _reportService;
    private readonly ILogger<ComplianceController> _logger;
    
    public ComplianceController(
        IComplianceReportService reportService,
        ILogger<ComplianceController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }
    
    [HttpGet("report")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateComplianceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value!);
        
        _logger.LogInformation("Generating CNDP compliance report for period {StartDate} to {EndDate} by admin {AdminId}",
            startDate, endDate, adminUserId);
        
        var reportPdf = await _reportService.GenerateComplianceReportAsync(
            startDate, 
            endDate, 
            adminUserId);
        
        var fileName = $"CNDP_Compliance_Report_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.pdf";
        
        return File(reportPdf, "application/pdf", fileName);
    }
}
