using System;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SilverbridgeWeb.Api.Database;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("silverbridgeDb");

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
}

app.UseHttpsRedirection();

app.MapGet("/api/ping", () => Results.Ok("Pong from Backend API 🚀"));

await app.RunAsync().ConfigureAwait(false);
