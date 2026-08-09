using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using Teams.Data.Context.Configuration.Helpers;
using Teams.Domain.Entities;

namespace Teams.Data.Context.Configuration;

[ExcludeFromCodeCoverage]
public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ConfigureEntityBase()
            .ToTable("invitations");

        builder.Property(e => e.GameId)
            .HasColumnName("game_id")
            .HasMaxLength(36);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(36);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<int>();

        builder.Property(e => e.EmailAddress)
            .HasColumnName("email")
            .HasMaxLength(1000);
        
        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error")
            .HasMaxLength(1000);

        // If the game is deleted, delete the invitations. We use soft deletion anyway, so this shouldn't really matter.
        builder.HasOne(p => p.Game)
            .WithMany()
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // The user relationship is not required (that's how we'll invite new users!) but when a user is deleted, we 
        // should delete their invitations by cascade
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}