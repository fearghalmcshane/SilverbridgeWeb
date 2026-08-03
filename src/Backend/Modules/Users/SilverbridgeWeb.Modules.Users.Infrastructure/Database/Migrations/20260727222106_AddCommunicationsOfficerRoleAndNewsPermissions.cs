using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer static readonly fields over constant array arguments

namespace SilverbridgeWeb.Modules.Users.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddCommunicationsOfficerRoleAndNewsPermissions : Migration
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
                "news:categories:update",
                "news:create",
                "news:delete",
                "news:media:upload",
                "news:publish",
                "news:read",
                "news:search",
                "news:update"
            });

        migrationBuilder.InsertData(
            schema: "users",
            table: "roles",
            column: "name",
            value: "Communications Officer");

        migrationBuilder.InsertData(
            schema: "users",
            table: "role_permissions",
            columns: new[] { "permission_code", "role_name" },
            values: new object[,]
            {
                { "news:categories:update", "Administrator" },
                { "news:categories:update", "Communications Officer" },
                { "news:create", "Administrator" },
                { "news:create", "Communications Officer" },
                { "news:delete", "Administrator" },
                { "news:delete", "Communications Officer" },
                { "news:media:upload", "Administrator" },
                { "news:media:upload", "Communications Officer" },
                { "news:publish", "Administrator" },
                { "news:publish", "Communications Officer" },
                { "news:read", "Administrator" },
                { "news:read", "Communications Officer" },
                { "news:search", "Administrator" },
                { "news:search", "Communications Officer" },
                { "news:update", "Administrator" },
                { "news:update", "Communications Officer" }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:categories:update", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:categories:update", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:create", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:create", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:delete", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:delete", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:media:upload", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:media:upload", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:publish", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:publish", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:read", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:read", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:search", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:search", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:update", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "news:update", "Communications Officer" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:categories:update");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:create");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:delete");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:media:upload");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:publish");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:read");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:search");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "news:update");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "roles",
            keyColumn: "name",
            keyValue: "Communications Officer");
    }
}
