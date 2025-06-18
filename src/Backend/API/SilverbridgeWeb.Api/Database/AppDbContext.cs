using Microsoft.EntityFrameworkCore;

namespace SilverbridgeWeb.Api.Database;

internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
