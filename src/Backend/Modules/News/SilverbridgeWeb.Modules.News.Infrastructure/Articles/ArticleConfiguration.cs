using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Articles;

internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.Excerpt)
            .HasMaxLength(1000);

        builder.Property(a => a.FeaturedImage)
            .HasMaxLength(1000)
            .HasColumnName("featured_image_url");

        builder.Property(a => a.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(a => a.PublishedAtUtc)
            .HasColumnName("published_at_utc");

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasMany<Category>().WithMany();

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.PublishedAtUtc);
        builder.HasIndex(a => a.AuthorId);
    }
}
