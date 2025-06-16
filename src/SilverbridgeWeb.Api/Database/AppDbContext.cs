using Microsoft.EntityFrameworkCore;

namespace SilverbridgeWeb.Api.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
