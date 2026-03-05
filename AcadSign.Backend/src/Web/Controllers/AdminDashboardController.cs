using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AcadSign.Backend.Application.Interfaces;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IDashboardMetricsService _metricsService;
    private readonly ILogger<AdminDashboardController> _logger;
    
    public AdminDashboardController(
        IDashboardMetricsService metricsService,
        ILogger<AdminDashboardController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }
    
    [HttpGet("dashboard")]
    [Produces("text/html")]
    public IActionResult GetDashboard()
    {
        return PhysicalFile(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "admin-dashboard.html"),
            "text/html");
    }
    
    [HttpGet("api/metrics")]
    [ProducesResponseType(typeof(DashboardMetrics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _metricsService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }
}

public class DashboardMetrics
{
    public RealtimeMetrics Realtime { get; set; }
    public ServiceStatus Services { get; set; }
    public CertificateStatus Certificate { get; set; }
    public HangfireStatus Hangfire { get; set; }
}

public class RealtimeMetrics
{
    public int DocumentsGeneratedToday { get; set; }
    public int DocumentsSignedToday { get; set; }
    public double SignatureSuccessRate { get; set; }
    public double AverageSignatureTimeSeconds { get; set; }
    public long StorageUsedGB { get; set; }
    public long StorageCapacityGB { get; set; }
    public double StorageUsagePercent { get; set; }
}

public class ServiceStatus
{
    public ServiceHealth BackendAPI { get; set; }
    public ServiceHealth PostgreSQL { get; set; }
    public ServiceHealth MinIO { get; set; }
    public ServiceHealth Seq { get; set; }
}

public class ServiceHealth
{
    public string Status { get; set; }
    public double UptimePercent { get; set; }
    public string Message { get; set; }
}

public class CertificateStatus
{
    public string Issuer { get; set; }
    public DateTime ValidUntil { get; set; }
    public int DaysRemaining { get; set; }
    public string Status { get; set; }
}

public class HangfireStatus
{
    public int JobsProcessing { get; set; }
    public int JobsSucceeded24h { get; set; }
    public int JobsFailed24h { get; set; }
    public int DeadLetterQueueSize { get; set; }
}
