using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Categories;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();

        builder.Property(c => c.IsArchived).IsRequired();

        builder.HasData(
            new
            {
                Id = Guid.Parse("0f2476a3-1d9d-4acd-8705-5b995702d002"),
                Name = "Club News",
                IsArchived = false
            },
            new
            {
                Id = Guid.Parse("5f5d6ab0-4b97-4f04-a658-2cf67dfbf60f"),
                Name = "Community",
                IsArchived = false
            },
            new
            {
                Id = Guid.Parse("83eafc8f-b4b6-4f4f-8f7c-4f7e15371641"),
                Name = "Match Reports",
                IsArchived = false
            },
            new
            {
                Id = Guid.Parse("bf2cfe8d-7388-4a16-b8f3-1047ea06bb65"),
                Name = "Fundraising",
                IsArchived = false
            });
    }
}
