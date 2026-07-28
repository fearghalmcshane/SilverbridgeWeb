using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Articles;

internal sealed class ArticleMediaConfiguration : IEntityTypeConfiguration<ArticleMedia>
{
    public void Configure(EntityTypeBuilder<ArticleMedia> builder)
    {
        builder.ToTable("article_media");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.BlobUrl).HasMaxLength(2048).IsRequired();

        builder.Property(m => m.MediaType).HasMaxLength(20).IsRequired();

        builder.Property(m => m.AltText).HasMaxLength(500);

        builder.Property(m => m.DisplayOrder).IsRequired();

        builder.HasIndex(m => new { m.ArticleId, m.DisplayOrder });
    }
}
