using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Attendance.Domain.Attendees;
using SilverbridgeWeb.Modules.Attendance.Domain.Events;
using SilverbridgeWeb.Modules.Attendance.Domain.Tickets;

namespace SilverbridgeWeb.Modules.Attendance.Infrastructure.Tickets;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code).HasMaxLength(30);

        builder.HasIndex(t => t.Code).IsUnique();

        builder.HasOne<Attendee>().WithMany().HasForeignKey(t => t.AttendeeId);

        builder.HasOne<Event>().WithMany().HasForeignKey(t => t.EventId);
    }
}
