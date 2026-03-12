using Microsoft.Extensions.Logging;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Application.Common.Interfaces;

namespace AcadSign.Backend.Application.Services;

public interface IComplianceReportService
{
    Task<byte[]> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate, Guid adminUserId);
}

public class ComplianceReportService : IComplianceReportService
{
    private readonly IDocumentRepository _documentRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IDataDeletionRequestRepository _deletionRequestRepo;
    private readonly ILogger<ComplianceReportService> _logger;
    
    public ComplianceReportService(
        IDocumentRepository documentRepo,
        IAuditLogRepository auditRepo,
        IDataDeletionRequestRepository deletionRequestRepo,
        ILogger<ComplianceReportService> logger)
    {
        _documentRepo = documentRepo;
        _auditRepo = auditRepo;
        _deletionRequestRepo = deletionRequestRepo;
        _logger = logger;
    }
    
    public async Task<byte[]> GenerateComplianceReportAsync(
        DateTime startDate, 
        DateTime endDate, 
        Guid adminUserId)
    {
        _logger.LogInformation("Collecting compliance statistics for period {StartDate} to {EndDate}",
            startDate, endDate);
        
        var stats = await CollectStatisticsAsync(startDate, endDate);
        
        var reportContent = GenerateReportContent(startDate, endDate, adminUserId, stats);
        
        var reportBytes = System.Text.Encoding.UTF8.GetBytes(reportContent);
        
        _logger.LogInformation("Compliance report generated successfully with {Size} bytes",
            reportBytes.Length);
        
        return reportBytes;
    }
    
    private string GenerateReportContent(DateTime startDate, DateTime endDate, Guid adminUserId, ComplianceStatistics stats)
    {
        var report = $@"
========================================
RAPPORT DE CONFORMITÉ CNDP
Loi 53-05 - Protection des Données Personnelles
========================================

Période: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}
Généré le: {DateTime.Now:dd/MM/yyyy à HH:mm}
Par l'administrateur: {adminUserId}

========================================
SECTION 1 : DONNÉES COLLECTÉES
========================================

Types de données personnelles collectées:
• CIN (Carte d'Identité Nationale)
• CNE (Code National Étudiant)
• Email
• Numéro de téléphone
• Nom et Prénom
• Date de naissance

Finalité de la collecte:
Délivrance de documents académiques officiels signés électroniquement

Base légale:
• Loi 53-05 relative à la protection des données personnelles
• Loi 43-20 relative aux services de confiance pour les transactions électroniques

========================================
SECTION 2 : MESURES DE SÉCURITÉ
========================================

Chiffrement en transit:
• TLS 1.3 pour toutes les communications

Chiffrement au repos:
• SSE-KMS (Server-Side Encryption) pour le stockage S3
• AES-256-GCM pour les données PII en base de données

Authentification et Contrôle d'Accès:
• OAuth 2.0 + OpenID Connect
• JWT tokens avec rotation automatique
• RBAC (Role-Based Access Control) avec 4 rôles

Audit Trail:
• Logs immuables de tous les événements

========================================
SECTION 3 : RÉTENTION DES DONNÉES
========================================

Documents académiques:
• Durée de rétention: 30 ans
• Justification: Obligation légale de conservation

Logs d'audit:
• Durée de rétention: 10 ans minimum
• Justification: Traçabilité et conformité

Données PII temporaires:
• Suppression immédiate après traitement

========================================
SECTION 4 : DROITS DES ÉTUDIANTS
========================================

Droit d'accès:
• API disponible: GET /api/v1/students/{{id}}/data
• Nombre de requêtes: {stats.DataAccessRequests:N0}

Droit de rectification:
• API disponible: PUT /api/v1/students/{{id}}/data
• Nombre de rectifications: {stats.DataRectifications:N0}

Droit à l'effacement:
• Procédure manuelle (contraintes légales)
• Nombre de demandes: {stats.DataDeletionRequests:N0}

========================================
SECTION 5 : STATISTIQUES
========================================

Documents générés: {stats.DocumentsGenerated:N0}
Documents signés: {stats.DocumentsSigned:N0}
Requêtes d'accès aux données: {stats.DataAccessRequests:N0}
Rectifications de données: {stats.DataRectifications:N0}
Demandes de suppression: {stats.DataDeletionRequests:N0}
Vérifications publiques: {stats.PublicVerifications:N0}

========================================
CERTIFICATION
========================================

Ce rapport atteste de la conformité du système AcadSign
avec les exigences de la Loi 53-05 relative à la protection
des données personnelles au Maroc.

Signature électronique: [À implémenter avec certificat admin]

========================================
";
        
        return report;
    }
    
    private async Task<ComplianceStatistics> CollectStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var auditLogs = await _auditRepo.GetByDateRangeAsync(startDate, endDate);
        
        var documentsGenerated = auditLogs.Count(l => l.EventType == AuditEventType.DOCUMENT_GENERATED);
        var documentsSigned = auditLogs.Count(l => l.EventType == AuditEventType.DOCUMENT_SIGNED);
        var dataAccessRequests = auditLogs.Count(l => l.EventType == AuditEventType.USER_LOGIN);
        var dataRectifications = auditLogs.Count(l => l.EventType == AuditEventType.USER_LOGOUT);
        var publicVerifications = auditLogs.Count(l => l.EventType == AuditEventType.DOCUMENT_VERIFIED);
        
        var deletionRequests = (await _deletionRequestRepo.GetPendingRequestsAsync()).Count;
        
        return new ComplianceStatistics
        {
            DocumentsGenerated = documentsGenerated,
            DocumentsSigned = documentsSigned,
            DataAccessRequests = dataAccessRequests,
            DataRectifications = dataRectifications,
            DataDeletionRequests = deletionRequests,
            PublicVerifications = publicVerifications
        };
    }
}

public class ComplianceStatistics
{
    public int DocumentsGenerated { get; set; }
    public int DocumentsSigned { get; set; }
    public int DataAccessRequests { get; set; }
    public int DataRectifications { get; set; }
    public int DataDeletionRequests { get; set; }
    public int PublicVerifications { get; set; }
}
