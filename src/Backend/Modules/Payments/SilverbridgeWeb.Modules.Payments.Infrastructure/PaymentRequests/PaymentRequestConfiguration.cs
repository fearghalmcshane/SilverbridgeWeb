using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

internal sealed class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
{
    internal const int ProviderIdMaxLength = 255;

    public void Configure(EntityTypeBuilder<PaymentRequest> builder)
    {
        builder.ToTable("payment_requests");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Source).HasMaxLength(PaymentRequest.SourceMaxLength);
        builder.Property(p => p.SourceReference).HasMaxLength(PaymentRequest.SourceReferenceMaxLength);
        builder.Property(p => p.Description).HasMaxLength(PaymentRequest.DescriptionMaxLength);
        builder.Property(p => p.Currency).HasMaxLength(3);
        builder.Property(p => p.FailureReason).HasMaxLength(PaymentRequest.ReasonMaxLength);
        builder.Property(p => p.ProviderCheckoutSessionId).HasMaxLength(ProviderIdMaxLength);
        builder.Property(p => p.CheckoutUrl).HasMaxLength(2048);
        builder.Property(p => p.ProviderPaymentIntentId).HasMaxLength(ProviderIdMaxLength);
        builder.Property(p => p.ProviderCustomerId).HasMaxLength(ProviderIdMaxLength);
        builder.Property(p => p.ProviderSubscriptionId).HasMaxLength(ProviderIdMaxLength);

        // Webhooks, reconciliation and payers can update the same request at once; the loser retries.
        builder.Property<uint>("Version").IsRowVersion();

        builder.HasIndex(p => p.PayerUserId);
        builder.HasIndex(p => new { p.Source, p.SourceReference });
        builder.HasIndex(p => p.ProviderPaymentIntentId);
        builder.HasIndex(p => new { p.Status, p.UpdatedAtUtc });

        builder.HasMany(p => p.Lines).WithOne().HasForeignKey(l => l.PaymentRequestId);
        builder.HasMany(p => p.Refunds).WithOne().HasForeignKey(r => r.PaymentRequestId);
    }
}
