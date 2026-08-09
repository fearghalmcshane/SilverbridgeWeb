using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable IDE0161 // Convert to file-scoped namespace

namespace SilverbridgeWeb.Modules.Bookings.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingContactName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "contact_name",
                schema: "bookings",
                table: "bookings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE bookings.bookings
                SET contact_name = booker_name
                WHERE contact_name = '';

                ALTER TABLE bookings.bookings
                ALTER COLUMN contact_name DROP DEFAULT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contact_name",
                schema: "bookings",
                table: "bookings");
        }
    }
}
