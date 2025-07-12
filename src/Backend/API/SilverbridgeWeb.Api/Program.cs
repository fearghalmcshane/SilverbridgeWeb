using Scalar.AspNetCore;
using SilverbridgeWeb.Api.Extensions;
using SilverbridgeWeb.Api.Middleware;
using SilverbridgeWeb.Common.Application;
using SilverbridgeWeb.Common.Infrastructure;
using SilverbridgeWeb.Modules.Events.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplication([SilverbridgeWeb.Modules.Events.Application.AssemblyReference.Assembly]);

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("silverbridgeDb")!,
    builder.Configuration.GetConnectionString("redis")!);

builder.Configuration.AddModuleConfiguration(["events"]);

builder.AddEventsModule();

builder.Services.AddCors();
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
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

EventsModule.MapEndpoints(app);

app.UseExceptionHandler();

await app.RunAsync().ConfigureAwait(false);
