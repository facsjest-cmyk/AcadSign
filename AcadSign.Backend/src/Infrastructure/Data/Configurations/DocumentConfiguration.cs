using AcadSign.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcadSign.Backend.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasIndex(d => d.PublicId)
            .IsUnique();

        builder.Property(d => d.PublicId)
            .HasDefaultValueSql("gen_random_uuid()");
    }
}
