using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer static readonly fields over constant array arguments
#pragma warning disable IDE0161 // Convert to file-scoped namespace

namespace SilverbridgeWeb.Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "users",
                table: "permissions",
                column: "code",
                values: new object[]
                {
                    "payments:manage",
                    "payments:read",
                    "payments:refund"
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "roles",
                column: "name",
                value: "Treasurer");

            migrationBuilder.InsertData(
                schema: "users",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "payments:manage", "Administrator" },
                    { "payments:manage", "Treasurer" },
                    { "payments:read", "Administrator" },
                    { "payments:read", "Treasurer" },
                    { "payments:refund", "Administrator" },
                    { "payments:refund", "Treasurer" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "payments:manage", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "payments:manage", "Treasurer" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "payments:read", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "payments:read", "Treasurer" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "payments:refund", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "payments:refund", "Treasurer" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "payments:manage");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "payments:read");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "payments:refund");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "roles",
                keyColumn: "name",
                keyValue: "Treasurer");
        }
    }
}
