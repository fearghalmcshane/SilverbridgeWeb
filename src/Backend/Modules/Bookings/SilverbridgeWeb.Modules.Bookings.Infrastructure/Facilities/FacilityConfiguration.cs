using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Infrastructure.Facilities;

internal sealed class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    private static readonly Guid TopFieldId = new("0198a3d7-0f62-7f54-8a3d-1d93747a0001");
    private static readonly Guid BigFieldId = new("0198a3d7-0f62-7f54-8a3d-1d93747a0002");
    private static readonly Guid WeeFieldId = new("0198a3d7-0f62-7f54-8a3d-1d93747a0003");
    private static readonly Guid HallId = new("0198a3d7-0f62-7f54-8a3d-1d93747a0004");
    private static readonly Guid BowlsRoomId = new("0198a3d7-0f62-7f54-8a3d-1d93747a0005");

    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(f => f.Name).IsUnique();

        builder.Property(f => f.Description).HasMaxLength(1000).IsRequired();

        builder.Property(f => f.Color).HasMaxLength(7).IsRequired();

        builder.HasData(
            new { Id = TopFieldId, Name = "Top Field", Description = "Top playing field", Color = "#2E7D32" },
            new { Id = BigFieldId, Name = "Big Field", Description = "Main playing field", Color = "#1565C0" },
            new { Id = WeeFieldId, Name = "Wee Field", Description = "Small playing field", Color = "#EF6C00" },
            new { Id = HallId, Name = "Hall", Description = "Main club hall", Color = "#6A1B9A" },
            new { Id = BowlsRoomId, Name = "Bowls Room", Description = "Indoor bowls room", Color = "#00838F" });
    }
}
