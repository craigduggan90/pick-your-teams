using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teams.Data.Context.Configuration.Helpers;
using Teams.Domain.Entities;

namespace Teams.Data.Context.Configuration;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ConfigureEntityBase()
            .ToTable("players");

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(100);

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(100);

        builder.Property(entity => entity.Rating)
            .HasColumnName("rating");
    }
}