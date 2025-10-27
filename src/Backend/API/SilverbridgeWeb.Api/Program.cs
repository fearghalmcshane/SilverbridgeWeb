﻿using Scalar.AspNetCore;
using SilverbridgeWeb.Api.Extensions;
using SilverbridgeWeb.Api.Middleware;
using SilverbridgeWeb.Common.Application;
using SilverbridgeWeb.Common.Infrastructure;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Events.Infrastructure;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure;
using SilverbridgeWeb.Modules.Users.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplication([
    SilverbridgeWeb.Modules.Events.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.Users.Application.AssemblyReference.Assembly,
    SilverbridgeWeb.Modules.Ticketing.Application.AssemblyReference.Assembly]);

builder.Services.AddInfrastructure(
    [TicketingModule.ConfigureConsumers],
    builder.Configuration.GetConnectionString("silverbridgeDb")!,
    builder.Configuration.GetConnectionString("redis")!,
    builder.Configuration["Users:Jwt:Key"]!,
    builder.Configuration["Users:Jwt:Issuer"]!,
    builder.Configuration["Users:Jwt:Audience"]!);

builder.Configuration.AddModuleConfiguration(["events", "users", "ticketing"]);

builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddTicketingModule(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseCors("AllowBlazorFrontend");
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync().ConfigureAwait(false);
