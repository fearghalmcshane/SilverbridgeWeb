using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Matches;

namespace SilverbridgeWeb.Modules.Matches.Infrastructure.Database;

internal sealed class MatchCompletionConfiguration : IEntityTypeConfiguration<MatchCompletion>
{
    public void Configure(EntityTypeBuilder<MatchCompletion> builder)
    {
        builder.ToTable("match_completions");
        builder.HasKey(completion => completion.Id);
        builder.Property(completion => completion.Id).ValueGeneratedNever();
        builder.Property(completion => completion.ProviderEventId).IsRequired().HasMaxLength(300);
        builder.Property(completion => completion.ProviderMatchId).IsRequired().HasMaxLength(300);
        builder.Property(completion => completion.EventType).IsRequired().HasMaxLength(100);
        builder.Property(completion => completion.ApiVersion).IsRequired().HasMaxLength(100);
        builder.Property(completion => completion.Status).IsRequired().HasMaxLength(100);
        builder.Property(completion => completion.Sport).IsRequired().HasMaxLength(200);
        builder.Property(completion => completion.Competition).HasMaxLength(300);
        builder.Property(completion => completion.Stage).HasMaxLength(300);
        builder.Property(completion => completion.VenueName).HasMaxLength(300);
        builder.Property(completion => completion.VenueType).HasMaxLength(100);
        builder.Property(completion => completion.TeamName).IsRequired().HasMaxLength(300);
        builder.Property(completion => completion.TeamScoreDisplay).IsRequired().HasMaxLength(100);
        builder.Property(completion => completion.OpponentName).IsRequired().HasMaxLength(300);
        builder.Property(completion => completion.OpponentScoreDisplay).IsRequired().HasMaxLength(100);
        builder.Property(completion => completion.Result).IsRequired().HasMaxLength(100);
        builder.Property(completion => completion.Summary).HasMaxLength(4000);
        builder.Property(completion => completion.LiveUrl).HasMaxLength(2000);
        builder.Property(completion => completion.OriginalJson).IsRequired().HasColumnType("text");
        builder.HasIndex(completion => completion.ProviderMatchId)
            .IsUnique()
            .HasDatabaseName(MatchCompletionRepository.ProviderMatchUniqueConstraint);
        builder.HasIndex(completion => completion.ProviderEventId)
            .IsUnique()
            .HasDatabaseName(MatchCompletionRepository.ProviderEventUniqueConstraint);
    }
}
