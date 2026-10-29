using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Subscriptions;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Source).HasMaxLength(PaymentRequest.SourceMaxLength);
        builder.Property(s => s.SourceReference).HasMaxLength(PaymentRequest.SourceReferenceMaxLength);
        builder.Property(s => s.Description).HasMaxLength(PaymentRequest.DescriptionMaxLength);
        builder.Property(s => s.Currency).HasMaxLength(3);
        builder.Property(s => s.ProviderSubscriptionId).HasMaxLength(PaymentRequestConfiguration.ProviderIdMaxLength);
        builder.Property(s => s.ProviderCustomerId).HasMaxLength(PaymentRequestConfiguration.ProviderIdMaxLength);

        builder.Property<uint>("Version").IsRowVersion();

        builder.HasIndex(s => s.ProviderSubscriptionId).IsUnique();
        builder.HasIndex(s => s.PayerUserId);

        builder.HasOne<PaymentRequest>().WithOne().HasForeignKey<Subscription>(s => s.Id);
        builder.HasMany(s => s.Payments).WithOne().HasForeignKey(p => p.SubscriptionId);
    }
}
