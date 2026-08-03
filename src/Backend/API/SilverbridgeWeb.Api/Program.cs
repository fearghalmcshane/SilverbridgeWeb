using Scalar.AspNetCore;
using SilverbridgeWeb.Api.Extensions;
using SilverbridgeWeb.Api.Middleware;
using SilverbridgeWeb.Common.Application;
using SilverbridgeWeb.Common.Infrastructure;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Attendance.Infrastructure;
using SilverbridgeWeb.Modules.Bookings.Infrastructure;
using SilverbridgeWeb.Modules.Events.Infrastructure;
using SilverbridgeWeb.Modules.Foireann.Infrastructure;
using SilverbridgeWeb.Modules.News.Infrastructure;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure;
using SilverbridgeWeb.Modules.Users.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplication([
    SilverbridgeWeb.Modules.Events.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.Users.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.Ticketing.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.Attendance.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.Foireann.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.Bookings.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.News.Application.AssemblyReference.Assembly]);

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("silverbridgeDb")!,
    builder.Configuration.GetConnectionString("redis")!,
    builder.Configuration["Clerk:Authority"]!
);

builder.Configuration.AddModuleConfiguration(["events", "users", "ticketing", "attendance", "foireann", "bookings", "news"]);

builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddTicketingModule(builder.Configuration);
builder.Services.AddAttendanceModule(builder.Configuration);
builder.Services.AddFoireannModule(builder.Configuration);
builder.Services.AddBookingsModule(builder.Configuration);
builder.Services.AddNewsModule(builder.Configuration);

builder.Services.AddCors();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseCors("AllowBlazorFrontend");
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod());
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapEndpoints();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

await app.RunAsync().ConfigureAwait(false);
