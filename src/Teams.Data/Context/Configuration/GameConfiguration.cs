using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teams.Data.Context.Configuration.Helpers;
using Teams.Data.Context.Converters;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.Context.Configuration;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ConfigureEntityBase()
            .ToTable("game");

        builder.Property(entity => entity.Location)
            .HasColumnName("location")
            .HasMaxLength(100);

        builder.Property(entity => entity.StartTime)
            .HasColumnName("start_time")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(entity => entity.EndTime)
            .HasColumnName("end_time")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(entity => entity.TeamSize)
            .HasColumnName("team_size");

        builder.Property(entity => entity.HomeTeamRating)
            .HasColumnName("home_rating");

        builder.Property(entity => entity.AwayTeamRating)
            .HasColumnName("away_rating");

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(
                toProvider => (int)toProvider,
                fromProvider => (GameStatusEnum)fromProvider);

        builder.Property(entity => entity.Winner)
            .HasColumnName("winner")
            .HasConversion(
                toProvider => (int?)toProvider,
                fromProvider => (GameTeamEnum?)fromProvider);
    }
}