namespace AcadSign.Backend.Application.Models;

public class DashboardMetrics
{
    public RealtimeMetrics Realtime { get; set; } = null!;
    public ServiceStatus Services { get; set; } = null!;
    public CertificateStatusInfo Certificate { get; set; } = null!;
    public HangfireStatusInfo Hangfire { get; set; } = null!;
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
    public ServiceHealthInfo BackendAPI { get; set; } = null!;
    public ServiceHealthInfo PostgreSQL { get; set; } = null!;
    public ServiceHealthInfo MinIO { get; set; } = null!;
    public ServiceHealthInfo Seq { get; set; } = null!;
}

public class ServiceHealthInfo
{
    public string Status { get; set; } = string.Empty;
    public double UptimePercent { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CertificateStatusInfo
{
    public string Status { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Issuer { get; set; } = string.Empty;
}

public class HangfireStatusInfo
{
    public string Status { get; set; } = string.Empty;
    public int JobsProcessed { get; set; }
    public int JobsQueued { get; set; }
    public int JobsFailed { get; set; }
}
