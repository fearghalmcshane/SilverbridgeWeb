using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SilverbridgeWeb.Common.Application;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Migrator;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Database;
using SilverbridgeWeb.Modules.Bookings.Infrastructure.Database;
using SilverbridgeWeb.Modules.Events.Infrastructure.Database;
using SilverbridgeWeb.Modules.Users.Application;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Database;
using SilverbridgeWeb.Modules.Users.Infrastructure;
using SilverbridgeWeb.Modules.Users.Infrastructure.Database;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigratorWorker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(MigratorWorker.ActivitySourceName));

builder.Services.AddApplication([
    AssemblyReference.Assembly
]);

builder.Services.AddSingleton<InsertOutboxMessagesInterceptor>();
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddHttpClient<ClerkUserBackfillService>();

string connectionString = builder.Configuration.GetConnectionString("silverbridgeDb")!;

builder.Services.AddDbContext<EventsDbContext>(options =>
    options
        .UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "events"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddDbContext<TicketingDbContext>(options =>
    options
        .UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "ticketing"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddDbContext<AttendanceDbContext>(options =>
    options
        .UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "attendance"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddDbContext<BookingsDbContext>(options =>
    options
        .UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "bookings"))
        .UseSnakeCaseNamingConvention());

await builder.Build().RunAsync();
