using AcadSign.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcadSign.Backend.Infrastructure.Data.Configurations;

public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
{
    public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.InstitutionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.TemplateData)
            .IsRequired();

        builder.Property(t => t.FileName)
            .HasMaxLength(255);

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.HasIndex(t => new { t.InstitutionId, t.Type, t.IsActive });
    }
}
