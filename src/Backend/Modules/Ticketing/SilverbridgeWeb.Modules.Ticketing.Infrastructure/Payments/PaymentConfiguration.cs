using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Ticketing.Domain.Orders;
using SilverbridgeWeb.Modules.Ticketing.Domain.Payments;

namespace SilverbridgeWeb.Modules.Ticketing.Infrastructure.Payments;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne<Order>().WithMany().HasForeignKey(p => p.OrderId);

        builder.HasIndex(p => p.TransactionId).IsUnique();
    }
}
