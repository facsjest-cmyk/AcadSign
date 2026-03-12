using AcadSign.Backend.Application.Models;

namespace AcadSign.Backend.Application.Interfaces;

public interface IDashboardMetricsService
{
    Task<DashboardMetrics> GetDashboardMetricsAsync();
}
