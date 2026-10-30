using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Subscriptions;

internal sealed class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.ToTable("subscription_payments");

        builder.HasKey(p => p.Id);

        // Payments are recorded in the domain with their own identifier, so EF must insert rather than update them.
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.ProviderInvoiceId).HasMaxLength(PaymentRequestConfiguration.ProviderIdMaxLength);
        builder.Property(p => p.FailureReason).HasMaxLength(PaymentRequest.ReasonMaxLength);

        builder.HasIndex(p => new { p.SubscriptionId, p.ProviderInvoiceId }).IsUnique();
    }
}
