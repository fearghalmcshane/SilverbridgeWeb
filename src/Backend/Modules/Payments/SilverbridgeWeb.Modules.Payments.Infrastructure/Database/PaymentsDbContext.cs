using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SilverbridgeWeb.Common.Infrastructure.Inbox;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Payers;
using SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Database;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<PaymentRequest> PaymentRequests { get; set; }

    internal DbSet<Subscription> Subscriptions { get; set; }

    internal DbSet<Payer> Payers { get; set; }

    internal DbSet<ProviderWebhookEvent> ProviderWebhookEvents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Payments);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentRequestConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentRequestLineConfiguration());
        modelBuilder.ApplyConfiguration(new RefundConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new PayerConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderWebhookEventConfiguration());
    }
}
