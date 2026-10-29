using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

internal sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");

        builder.HasKey(r => r.Id);

        // Refunds are created in the domain with their own identifier, so EF must insert rather than update them.
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Reason).HasMaxLength(PaymentRequest.ReasonMaxLength);
        builder.Property(r => r.FailureReason).HasMaxLength(PaymentRequest.ReasonMaxLength);
        builder.Property(r => r.ProviderRefundId).HasMaxLength(PaymentRequestConfiguration.ProviderIdMaxLength);

        builder.HasIndex(r => r.ProviderRefundId).IsUnique();
    }
}
