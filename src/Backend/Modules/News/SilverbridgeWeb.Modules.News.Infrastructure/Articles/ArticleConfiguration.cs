using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Articles;

internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).HasMaxLength(300).IsRequired();

        builder.Property(a => a.Slug).HasMaxLength(200).IsRequired();

        builder.Property(a => a.AuthorFirstName).HasMaxLength(200).IsRequired();

        builder.Property(a => a.AuthorLastName).HasMaxLength(200).IsRequired();

        builder.Property(a => a.Summary).HasMaxLength(1000).IsRequired();

        builder.Property(a => a.Content).IsRequired();

        builder.Property(a => a.Status).IsRequired();

        builder.Property(a => a.CreatedAtUtc).IsRequired();

        builder.HasIndex(a => a.Slug).IsUnique();

        builder.HasOne<Category>().WithMany().HasForeignKey(a => a.CategoryId);

        builder.HasMany(a => a.Media)
            .WithOne()
            .HasForeignKey(m => m.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Media).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
