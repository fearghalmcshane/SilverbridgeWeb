using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Users.Domain.Users;
using SilverbridgeWeb.Modules.Users.Infrastructure.Users;

namespace SilverbridgeWeb.Modules.Users.Infrastructure.Database;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schemas.Users);

        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new UserConfiguration());

        // Configure Identity tables to use snake_case and schema
        builder.Entity<User>().ToTable("users", Schemas.Users);
        builder.Entity<IdentityRole<Guid>>().ToTable("roles", Schemas.Users);
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", Schemas.Users);
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", Schemas.Users);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", Schemas.Users);
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", Schemas.Users);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", Schemas.Users);
    }
}
