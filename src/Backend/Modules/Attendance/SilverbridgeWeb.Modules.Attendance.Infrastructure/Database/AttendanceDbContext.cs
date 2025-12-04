using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Attendance.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Attendance.Domain.Attendees;
using SilverbridgeWeb.Modules.Attendance.Domain.Events;
using SilverbridgeWeb.Modules.Attendance.Domain.Tickets;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Attendees;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Events;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Tickets;

namespace SilverbridgeWeb.Modules.Attendance.Infrastructure.Database;

public sealed class AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Attendee> Attendees { get; set; }

    internal DbSet<Event> Events { get; set; }

    internal DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Attendance);

        modelBuilder.ApplyConfiguration(new AttendeeConfiguration());
        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
    }
}
