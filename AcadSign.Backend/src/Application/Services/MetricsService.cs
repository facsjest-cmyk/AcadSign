using Prometheus;

namespace AcadSign.Backend.Application.Services;

public class MetricsService
{
    private readonly Counter _documentsGeneratedTotal;
    private readonly Counter _documentsSignedTotal;
    private readonly Counter _signatureFailuresTotal;
    private readonly Counter _emailsSentTotal;
    
    private readonly Gauge _signatureSuccessRate;
    private readonly Gauge _certificateDaysRemaining;
    private readonly Gauge _storageUsageBytes;
    private readonly Gauge _storageCapacityBytes;
    private readonly Gauge _activeUsers;
    
    private readonly Histogram _signatureDuration;
    private readonly Histogram _pdfGenerationDuration;
    
    public MetricsService()
    {
        _documentsGeneratedTotal = Metrics.CreateCounter(
            "acadsign_documents_generated_total",
            "Total number of documents generated",
            new CounterConfiguration
            {
                LabelNames = new[] { "document_type" }
            });
        
        _documentsSignedTotal = Metrics.CreateCounter(
            "acadsign_documents_signed_total",
            "Total number of documents signed successfully");
        
        _signatureFailuresTotal = Metrics.CreateCounter(
            "acadsign_signature_failures_total",
            "Total number of signature failures",
            new CounterConfiguration
            {
                LabelNames = new[] { "error_type" }
            });
        
        _emailsSentTotal = Metrics.CreateCounter(
            "acadsign_emails_sent_total",
            "Total number of emails sent");
        
        _signatureSuccessRate = Metrics.CreateGauge(
            "acadsign_signature_success_rate",
            "Signature success rate (0-1)");
        
        _certificateDaysRemaining = Metrics.CreateGauge(
            "acadsign_certificate_days_remaining",
            "Days remaining until certificate expiry");
        
        _storageUsageBytes = Metrics.CreateGauge(
            "acadsign_storage_usage_bytes",
            "Storage usage in bytes",
            new GaugeConfiguration
            {
                LabelNames = new[] { "storage_type" }
            });
        
        _storageCapacityBytes = Metrics.CreateGauge(
            "acadsign_storage_capacity_bytes",
            "Storage capacity in bytes",
            new GaugeConfiguration
            {
                LabelNames = new[] { "storage_type" }
            });
        
        _activeUsers = Metrics.CreateGauge(
            "acadsign_active_users",
            "Number of active concurrent users");
        
        _signatureDuration = Metrics.CreateHistogram(
            "acadsign_signature_duration_seconds",
            "Signature operation duration in seconds",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
            });
        
        _pdfGenerationDuration = Metrics.CreateHistogram(
            "acadsign_pdf_generation_duration_seconds",
            "PDF generation duration in seconds",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
            });
    }
    
    public void IncrementDocumentsGenerated(string documentType)
    {
        _documentsGeneratedTotal.WithLabels(documentType).Inc();
    }
    
    public void IncrementDocumentsSigned()
    {
        _documentsSignedTotal.Inc();
    }
    
    public void IncrementSignatureFailures(string errorType)
    {
        _signatureFailuresTotal.WithLabels(errorType).Inc();
    }
    
    public void IncrementEmailsSent()
    {
        _emailsSentTotal.Inc();
    }
    
    public void RecordSignatureDuration(double seconds)
    {
        _signatureDuration.Observe(seconds);
    }
    
    public void RecordPdfGenerationDuration(double seconds)
    {
        _pdfGenerationDuration.Observe(seconds);
    }
    
    public void UpdateSignatureSuccessRate(double rate)
    {
        _signatureSuccessRate.Set(rate);
    }
    
    public void UpdateCertificateDaysRemaining(int days)
    {
        _certificateDaysRemaining.Set(days);
    }
    
    public void UpdateStorageUsage(string storageType, long bytes)
    {
        _storageUsageBytes.WithLabels(storageType).Set(bytes);
    }
    
    public void UpdateStorageCapacity(string storageType, long bytes)
    {
        _storageCapacityBytes.WithLabels(storageType).Set(bytes);
    }
    
    public void UpdateActiveUsers(int count)
    {
        _activeUsers.Set(count);
    }
}
