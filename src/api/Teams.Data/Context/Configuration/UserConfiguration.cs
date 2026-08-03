using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using Teams.Data.Context.Configuration.Helpers;
using Teams.Domain.Entities;

namespace Teams.Data.Context.Configuration;

[ExcludeFromCodeCoverage]
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ConfigureEntityBase().ToTable("users");
        builder.HasIndex(entity => entity.Tag).IsUnique();
        builder.HasIndex(entity => entity.EmailAddress).IsUnique();

        builder.Property(entity => entity.Tag)
            .HasColumnName("tag")
            .HasMaxLength(36);

        builder.Property(entity => entity.DisplayName)
            .HasColumnName("name")
            .HasMaxLength(100);

        builder.Property(entity => entity.ExternalId)
            .HasColumnName("idp_id")
            .HasMaxLength(255);

        builder.Property(entity => entity.Rating)
            .HasColumnName("rating");

        builder.Property(entity => entity.EmailAddress)
            .HasColumnName("email")
            .HasMaxLength(1000);

        builder.Property(entity => entity.Mobile)
            .HasColumnName("phone")
            .HasMaxLength(100);
    }
}