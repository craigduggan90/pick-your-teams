using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teams.Data.Context.Configuration.Helpers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Teams.Data.Context.Configuration;

/// <summary>Entity framework configuration for the <see cref="Job"/> type.</summary>
[ExcludeFromCodeCoverage]
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ConfigureEntityBase()
            .ToTable("jobs");

        builder.HasIndex(entity => entity.IdempotencyKey)
            .IsUnique()
            .HasFilter("date_deleted IS NULL");

        builder.Property(entity => entity.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100);

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(
                entity => (int)entity,
                stored => (JobStatusEnum)stored);

        builder.Property(entity => entity.Type)
            .HasColumnName("type")
            .HasConversion(
                entity => (int)entity,
                stored => (JobTypeEnum)stored);

        builder.Property(entity => entity.Parameters)
            .HasColumnName("parameters")
            .HasMaxLength(1000);

        builder.Property(entity => entity.ErrorCode)
            .HasColumnName("error_code")
            .HasMaxLength(50);

        builder.Property(entity => entity.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(255);

        builder.Property(entity => entity.ConcurrencyToken)
            .HasColumnName("concurrency_token")
            .HasMaxLength(32)
            .IsConcurrencyToken();
    }
}