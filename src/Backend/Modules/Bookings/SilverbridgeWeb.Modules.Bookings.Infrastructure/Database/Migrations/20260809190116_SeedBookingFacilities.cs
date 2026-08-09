using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable IDE0161 // Convert to file-scoped namespace

namespace SilverbridgeWeb.Modules.Bookings.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedBookingFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH duplicate_facilities AS
                (
                    SELECT id, ROW_NUMBER() OVER (PARTITION BY name ORDER BY id) AS duplicate_number
                    FROM bookings.facilities
                )
                UPDATE bookings.facilities AS facility
                SET name = LEFT(facility.name, 160) || ' (' || facility.id::text || ')'
                FROM duplicate_facilities AS duplicate
                WHERE facility.id = duplicate.id
                  AND duplicate.duplicate_number > 1;

                INSERT INTO bookings.facilities (id, name, description, color)
                SELECT '0198a3d7-0f62-7f54-8a3d-1d93747a0001', 'Top Field', 'Top playing field', '#2E7D32'
                WHERE NOT EXISTS (SELECT 1 FROM bookings.facilities WHERE name = 'Top Field');

                INSERT INTO bookings.facilities (id, name, description, color)
                SELECT '0198a3d7-0f62-7f54-8a3d-1d93747a0002', 'Big Field', 'Main playing field', '#1565C0'
                WHERE NOT EXISTS (SELECT 1 FROM bookings.facilities WHERE name = 'Big Field');

                INSERT INTO bookings.facilities (id, name, description, color)
                SELECT '0198a3d7-0f62-7f54-8a3d-1d93747a0003', 'Wee Field', 'Small playing field', '#EF6C00'
                WHERE NOT EXISTS (SELECT 1 FROM bookings.facilities WHERE name = 'Wee Field');

                INSERT INTO bookings.facilities (id, name, description, color)
                SELECT '0198a3d7-0f62-7f54-8a3d-1d93747a0004', 'Hall', 'Main club hall', '#6A1B9A'
                WHERE NOT EXISTS (SELECT 1 FROM bookings.facilities WHERE name = 'Hall');

                INSERT INTO bookings.facilities (id, name, description, color)
                SELECT '0198a3d7-0f62-7f54-8a3d-1d93747a0005', 'Bowls Room', 'Indoor bowls room', '#00838F'
                WHERE NOT EXISTS (SELECT 1 FROM bookings.facilities WHERE name = 'Bowls Room');

                CREATE EXTENSION IF NOT EXISTS btree_gist;

                DO $migration$
                DECLARE
                    conflicting_booking_id uuid;
                BEGIN
                    LOOP
                        conflicting_booking_id := NULL;

                        SELECT booking.id
                        INTO conflicting_booking_id
                        FROM bookings.bookings AS booking
                        WHERE booking.status <> 'Cancelled'
                          AND EXISTS
                          (
                              SELECT 1
                              FROM bookings.bookings AS earlier_booking
                              WHERE earlier_booking.facility_id = booking.facility_id
                                AND earlier_booking.status <> 'Cancelled'
                                AND earlier_booking.starts_at_utc < booking.ends_at_utc
                                AND earlier_booking.ends_at_utc > booking.starts_at_utc
                                AND
                                (
                                    earlier_booking.starts_at_utc < booking.starts_at_utc
                                    OR
                                    (
                                        earlier_booking.starts_at_utc = booking.starts_at_utc
                                        AND earlier_booking.id < booking.id
                                    )
                                )
                          )
                        ORDER BY booking.starts_at_utc, booking.id
                        LIMIT 1;

                        EXIT WHEN conflicting_booking_id IS NULL;

                        UPDATE bookings.bookings
                        SET status = 'Cancelled'
                        WHERE id = conflicting_booking_id;
                    END LOOP;
                END
                $migration$;

                ALTER TABLE bookings.bookings
                ADD CONSTRAINT ex_bookings_facility_time
                EXCLUDE USING gist
                (
                    facility_id WITH =,
                    tstzrange(starts_at_utc, ends_at_utc, '[)') WITH &&
                )
                WHERE (status <> 'Cancelled');
                """);

            migrationBuilder.CreateIndex(
                name: "ix_facilities_name",
                schema: "bookings",
                table: "facilities",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE bookings.bookings
                DROP CONSTRAINT IF EXISTS ex_bookings_facility_time;
                """);

            migrationBuilder.DropIndex(
                name: "ix_facilities_name",
                schema: "bookings",
                table: "facilities");

            migrationBuilder.DeleteData(
                schema: "bookings",
                table: "facilities",
                keyColumn: "id",
                keyValue: new Guid("0198a3d7-0f62-7f54-8a3d-1d93747a0001"));

            migrationBuilder.DeleteData(
                schema: "bookings",
                table: "facilities",
                keyColumn: "id",
                keyValue: new Guid("0198a3d7-0f62-7f54-8a3d-1d93747a0002"));

            migrationBuilder.DeleteData(
                schema: "bookings",
                table: "facilities",
                keyColumn: "id",
                keyValue: new Guid("0198a3d7-0f62-7f54-8a3d-1d93747a0003"));

            migrationBuilder.DeleteData(
                schema: "bookings",
                table: "facilities",
                keyColumn: "id",
                keyValue: new Guid("0198a3d7-0f62-7f54-8a3d-1d93747a0004"));

            migrationBuilder.DeleteData(
                schema: "bookings",
                table: "facilities",
                keyColumn: "id",
                keyValue: new Guid("0198a3d7-0f62-7f54-8a3d-1d93747a0005"));
        }
    }
}
