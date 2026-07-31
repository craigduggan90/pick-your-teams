using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using Teams.Data.Context.Configuration.Helpers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Context.Configuration;

[ExcludeFromCodeCoverage]
public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ConfigureEntityBase()
            .ToTable("players");

        builder.Property(e => e.GameId)
            .HasColumnName("game_id")
            .HasMaxLength(36);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(36);

        builder.Property(e => e.Rating)
            .HasColumnName("rating");

        builder.Property(e => e.RatingChange)
            .HasColumnName("delta");

        builder.Property(e => e.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100);

        builder.Property(e => e.Team)
            .HasColumnName("team")
            .HasConversion(
                e => (int)e,
                i => (GameTeamEnum)i);

        builder.Property(e => e.Type)
            .HasColumnName("type")
            .HasConversion(
                e => (int)e,
                i => (PlayerTypeEnum)i);

        // If the game is deleted, delete the players. We use soft deletion anyway, so this shouldn't really matter.
        builder.HasOne(p => p.Game)
            .WithMany(g => g.Players)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.User)
            .WithMany(g => g.Participation)
            .HasForeignKey(p => p.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}