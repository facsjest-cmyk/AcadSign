using AcadSign.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net;
using System.Text.Json;

namespace AcadSign.Backend.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.DocumentId)
            .HasColumnName("document_id")
            .HasColumnType("uuid");

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasColumnType("character varying(50)")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid");

        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasConversion(
                v => string.IsNullOrWhiteSpace(v) ? null : IPAddress.Parse(v),
                v => v == null ? null : v.ToString());

        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text");

        builder.Property(x => x.CertificateSerial)
            .HasColumnName("certificate_serial")
            .HasColumnType("character varying(100)");

        builder.Property(x => x.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : v.RootElement.GetRawText(),
                v => v == null ? null : JsonDocument.Parse(v));

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(x => x.DocumentId).HasDatabaseName("idx_audit_logs_document_id");
        builder.HasIndex(x => x.EventType).HasDatabaseName("idx_audit_logs_event_type");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_audit_logs_created_at");
        builder.HasIndex(x => x.UserId).HasDatabaseName("idx_audit_logs_user_id");
        builder.HasIndex(x => x.CorrelationId).HasDatabaseName("idx_audit_logs_correlation_id");
    }
}
