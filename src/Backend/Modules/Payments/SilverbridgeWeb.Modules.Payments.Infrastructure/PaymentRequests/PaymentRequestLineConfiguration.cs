using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

internal sealed class PaymentRequestLineConfiguration : IEntityTypeConfiguration<PaymentRequestLine>
{
    public void Configure(EntityTypeBuilder<PaymentRequestLine> builder)
    {
        builder.ToTable("payment_request_lines");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Description).HasMaxLength(PaymentRequest.DescriptionMaxLength);
    }
}
