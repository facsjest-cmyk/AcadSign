using AcadSign.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcadSign.Backend.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.Property(x => x.PublicId)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.HasIndex(x => x.PublicId)
            .IsUnique();
    }
}
