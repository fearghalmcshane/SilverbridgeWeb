using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;

internal sealed class ProviderWebhookEventConfiguration : IEntityTypeConfiguration<ProviderWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProviderWebhookEvent> builder)
    {
        builder.ToTable("provider_webhook_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasMaxLength(PaymentRequestConfiguration.ProviderIdMaxLength);
        builder.Property(e => e.Type).HasMaxLength(100);
        builder.Property(e => e.Payload).HasColumnType("text");

        builder.HasIndex(e => new { e.ProcessedAtUtc, e.ReceivedAtUtc });
    }
}
