using Scalar.AspNetCore;
using SilverbridgeWeb.Api.Extensions;
using SilverbridgeWeb.Modules.Events.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

await app.RunAsync().ConfigureAwait(false);
