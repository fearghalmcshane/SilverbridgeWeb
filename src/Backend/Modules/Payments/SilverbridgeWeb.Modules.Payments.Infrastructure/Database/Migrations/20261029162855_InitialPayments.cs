using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer static readonly fields over constant array arguments
#pragma warning disable IDE0161 // Convert to file-scoped namespace

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payments");

            migrationBuilder.CreateTable(
                name: "inbox_message_consumers",
                schema: "payments",
                columns: table => new
                {
                    inbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_message_consumers", x => new { x.inbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", maxLength: 2000, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message_consumers",
                schema: "payments",
                columns: table => new
                {
                    outbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message_consumers", x => new { x.outbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", maxLength: 2000, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payers",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    first_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    provider_customer_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_requests",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    amount_minor = table.Column<long>(type: "bigint", nullable: false),
                    amount_refunded_minor = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    provider_checkout_session_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    checkout_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    checkout_expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    provider_payment_intent_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider_customer_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider_subscription_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_webhook_events",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_webhook_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_request_lines",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_amount_minor = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_request_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_request_lines_payment_requests_payment_request_id",
                        column: x => x.payment_request_id,
                        principalSchema: "payments",
                        principalTable: "payment_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_minor = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    provider_refund_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refunds", x => x.id);
                    table.ForeignKey(
                        name: "fk_refunds_payment_requests_payment_request_id",
                        column: x => x.payment_request_id,
                        principalSchema: "payments",
                        principalTable: "payment_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount_minor = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    provider_subscription_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_customer_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    current_period_end_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_subscriptions_payment_requests_id",
                        column: x => x.id,
                        principalSchema: "payments",
                        principalTable: "payment_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_payments",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_invoice_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    amount_minor = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    period_start_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    period_end_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_subscription_payments_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalSchema: "payments",
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_request_lines_payment_request_id",
                schema: "payments",
                table: "payment_request_lines",
                column: "payment_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_payer_user_id",
                schema: "payments",
                table: "payment_requests",
                column: "payer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_provider_payment_intent_id",
                schema: "payments",
                table: "payment_requests",
                column: "provider_payment_intent_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_source_source_reference",
                schema: "payments",
                table: "payment_requests",
                columns: new[] { "source", "source_reference" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_requests_status_updated_at_utc",
                schema: "payments",
                table: "payment_requests",
                columns: new[] { "status", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_provider_webhook_events_processed_at_utc_received_at_utc",
                schema: "payments",
                table: "provider_webhook_events",
                columns: new[] { "processed_at_utc", "received_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_refunds_payment_request_id",
                schema: "payments",
                table: "refunds",
                column: "payment_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_provider_refund_id",
                schema: "payments",
                table: "refunds",
                column: "provider_refund_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscription_payments_subscription_id_provider_invoice_id",
                schema: "payments",
                table: "subscription_payments",
                columns: new[] { "subscription_id", "provider_invoice_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_payer_user_id",
                schema: "payments",
                table: "subscriptions",
                column: "payer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_provider_subscription_id",
                schema: "payments",
                table: "subscriptions",
                column: "provider_subscription_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "payers",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "payment_request_lines",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "provider_webhook_events",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "refunds",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "subscription_payments",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "payment_requests",
                schema: "payments");
        }
    }
}
