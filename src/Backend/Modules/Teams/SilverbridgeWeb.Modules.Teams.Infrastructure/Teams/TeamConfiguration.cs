using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Infrastructure.Teams;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.AgeGroup)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.CoachName)
            .HasMaxLength(200);

        builder.Property(t => t.IsActive)
            .IsRequired();

        builder.OwnsMany(t => t.SquadMembers, sm =>
        {
            sm.ToTable("squad_members");

            sm.Property(sm => sm.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            sm.Property(sm => sm.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            sm.Property(sm => sm.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            sm.Property(sm => sm.JerseyNumber)
                .HasColumnName("jersey_number");
        });

        builder.Navigation(t => t.SquadMembers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.HasIndex(t => t.AgeGroup);
    }
}
