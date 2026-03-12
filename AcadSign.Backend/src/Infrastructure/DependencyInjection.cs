using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Constants;
using AcadSign.Backend.Infrastructure.Data;
using AcadSign.Backend.Infrastructure.Data.Interceptors;
using AcadSign.Backend.Infrastructure.Data.Repositories;
using AcadSign.Backend.Infrastructure.Identity;
using AcadSign.Backend.Infrastructure.Pdf;
using AcadSign.Backend.Infrastructure.QrCode;
using AcadSign.Backend.Infrastructure.Security;
using AcadSign.Backend.Infrastructure.Storage;
using AcadSign.Backend.Infrastructure.Services;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Application.Interfaces;
using Minio;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("AcadSign.BackendDb");
        Guard.Against.Null(connectionString, message: "Connection string 'AcadSign.BackendDb' not found.");

        builder.Services.AddSingleton<AuditableEntityInterceptor>();
        builder.Services.AddSingleton<DispatchDomainEventsInterceptor>();
        builder.Services.AddSingleton<EncryptionInterceptor>();
        builder.Services.AddSingleton<DecryptionInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>(),
                sp.GetRequiredService<EncryptionInterceptor>(),
                sp.GetRequiredService<DecryptionInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddHttpClient();

        builder.Services.AddScoped<ISisAttestationExportClient, SisAttestationExportClient>();

        // Configure Data Protection API for PII encryption
        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>()
            .SetApplicationName("AcadSign")
            .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            })
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90)); // Rotation tous les 90 jours

        // Register PII Encryption Service
        builder.Services.AddSingleton<IPiiEncryptionService, PiiEncryptionService>();

        // Register QR Code Service
        builder.Services.AddSingleton<IQrCodeService, QrCodeService>();

        // Register attestation template renderer
        builder.Services.AddScoped<IAttestationTemplateRenderer, AttestationTemplateRenderer>();

        // Register PDF Generation Service
        builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();

        // Epic 6 - public verification
        builder.Services.AddScoped<ICertificateValidationService, CertificateValidationService>();
        builder.Services.AddScoped<ISignatureVerificationService, SignatureVerificationService>();
        builder.Services.AddScoped<IVerificationReportService, AcadSign.Backend.Infrastructure.Pdf.VerificationReportService>();

        // Epic 8 - audit trail
        builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();

        // Epic 9 - email notifications
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IStorageService, S3StorageAdapter>();
        builder.Services.AddScoped<AcadSign.Backend.Application.BackgroundJobs.EmailNotificationJob>();

        // Epic 5 repositories/services
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<CachedPdfGenerationService>();

        // Configure MinIO Client
        var minioConfig = builder.Configuration.GetSection("MinIO");
        builder.Services.AddSingleton<IMinioClient>(sp =>
        {
            var endpoint = minioConfig["Endpoint"] ?? "localhost:9000";
            var accessKey = minioConfig["AccessKey"] ?? "minioadmin";
            var secretKey = minioConfig["SecretKey"] ?? "minioadmin";
            var useSSL = bool.Parse(minioConfig["UseSSL"] ?? "false");

            return new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSSL)
                .Build();
        });

        // Register S3 Storage Service
        builder.Services.AddScoped<IS3StorageService, S3StorageService>();

        // Register Template Repository
        builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();

        // Epic 5 repositories/services
        builder.Services.AddScoped<AcadSign.Backend.Application.Services.IBatchRepository, BatchRepository>();
        builder.Services.AddScoped<AcadSign.Backend.Application.Services.IDeadLetterQueueRepository, DeadLetterQueueRepository>();
        builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
        builder.Services.AddScoped<BatchProcessingService>();
        builder.Services.AddScoped<DeadLetterQueueService>();

        builder.Services.AddScoped<IAttestationBatchGenerationService, AttestationBatchGenerationService>();

        // Register Auth Service
        builder.Services.AddScoped<AcadSign.Backend.Application.Auth.Services.IAuthService, AcadSign.Backend.Application.Auth.Services.AuthService>();

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }
}
