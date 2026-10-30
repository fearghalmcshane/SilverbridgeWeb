using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Payers;

internal sealed class PayerConfiguration : IEntityTypeConfiguration<Payer>
{
    public void Configure(EntityTypeBuilder<Payer> builder)
    {
        builder.ToTable("payers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Email).HasMaxLength(300);
        builder.Property(p => p.FirstName).HasMaxLength(200);
        builder.Property(p => p.LastName).HasMaxLength(200);
        builder.Property(p => p.ProviderCustomerId).HasMaxLength(PaymentRequestConfiguration.ProviderIdMaxLength);
    }
}
