using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Attendance.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Attendance.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Attendance.Domain.Attendees;
using SilverbridgeWeb.Modules.Attendance.Domain.Events;
using SilverbridgeWeb.Modules.Attendance.Domain.Tickets;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Attendees;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Authentication;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Database;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Events;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Tickets;
using SilverbridgeWeb.Modules.Attendance.Presentation.Attendees;
using SilverbridgeWeb.Modules.Attendance.Presentation.Events;
using SilverbridgeWeb.Modules.Attendance.Presentation.Tickets;

namespace SilverbridgeWeb.Modules.Attendance.Infrastructure;

public static class AttendanceModule
{
    public static IServiceCollection AddAttendanceModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        registrationConfigurator.AddConsumer<UserRegisteredIntegrationEventConsumer>();
        registrationConfigurator.AddConsumer<UserProfileUpdatedIntegrationEventConsumer>();
        registrationConfigurator.AddConsumer<EventPublishedIntegrationEventConsumer>();
        registrationConfigurator.AddConsumer<TicketIssuedIntegrationEventConsumer>();
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        services.AddDbContext<AttendanceDbContext>((sp, options) =>
            options.UseNpgsql(databaseConnectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Attendance))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AttendanceDbContext>());

        services.AddScoped<IAttendeeRepository, AttendeeRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();

        services.AddScoped<IAttendanceContext, AttendanceContext>();

        services.Configure<OutboxOptions>(configuration.GetSection("Attendance:Outbox"));

        services.ConfigureOptions<ConfigureProcessOutboxJob>();
    }
}
