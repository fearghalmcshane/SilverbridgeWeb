using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Users.Domain.Users;

namespace SilverbridgeWeb.Modules.Users.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(200).IsRequired();

        builder.Property(u => u.LastName).HasMaxLength(200).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
