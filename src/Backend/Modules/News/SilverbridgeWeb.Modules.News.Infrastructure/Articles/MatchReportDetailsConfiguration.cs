using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Articles;

internal sealed class MatchReportDetailsConfiguration : IEntityTypeConfiguration<MatchReportDetails>
{
    public void Configure(EntityTypeBuilder<MatchReportDetails> builder)
    {
        builder.ToTable("match_report_details");

        // Shared primary key with the owning Article - avoids introducing another client-generated key
        // that would need the same explicit-tracking workaround as ArticleMedia.
        builder.HasKey(m => m.ArticleId);

        builder.Property(m => m.HomeTeam).HasMaxLength(200).IsRequired();

        builder.Property(m => m.AwayTeam).HasMaxLength(200).IsRequired();

        builder.Property(m => m.HomeGoals).IsRequired();

        builder.Property(m => m.HomePoints).IsRequired();

        builder.Property(m => m.AwayGoals).IsRequired();

        builder.Property(m => m.AwayPoints).IsRequired();

        builder.Property(m => m.Competition).HasMaxLength(200);

        builder.Property(m => m.Venue).HasMaxLength(200);
    }
}
