using AcadSign.Backend.Web.Controllers;

namespace AcadSign.Backend.Application.Interfaces;

public interface IDashboardMetricsService
{
    Task<DashboardMetrics> GetDashboardMetricsAsync();
}
