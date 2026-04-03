using RevPay.Application;
using RevPay.Infrastructure;
using RevPay.API.Extensions;
using RevPay.API.Middleware;
using RevPay.Infrastructure.Jobs;
using Serilog;
using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/revpay-.txt", rollingInterval: RollingInterval.Day));

// ── Services ───────────────────────────────────────────────────────────────────
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── Rate Limiting ──────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(opts =>
{
    opts.AddFixedWindowLimiter("api", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    opts.RejectionStatusCode = 429;
});

// ── Health Checks ──────────────────────────────────────────────────────────────
builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Default")!,
        name: "postgres",
        tags: ["db", "ready"])
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379",
        name: "redis",
        tags: ["cache", "ready"])
    .AddHangfire(o => o.MinimumAvailableServers = 1,
        name: "hangfire",
        tags: ["jobs"]);

// ── CORS for citizen portal ────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddPolicy("CitizenPortal", p => p
        .WithOrigins(
            builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────────
app.UseExceptionHandler();
app.UseCors("CitizenPortal");
app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RevPay Nigeria API v1");
    c.RoutePrefix = string.Empty;         // Swagger at root "/"
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<IdempotencyMiddleware>();

app.MapControllers().RequireRateLimiting("api");
app.UseHangfireDashboard("/hangfire");

// ── Health endpoints ───────────────────────────────────────────────────────────
app.MapHealthChecks("/health/ready", new HealthCheckOptions
    { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new HealthCheckOptions
    { Predicate = _ => false });

// ── Hangfire recurring jobs ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    RecurringJob.AddOrUpdate<ReconciliationJob>(
        "daily-reconciliation",
        job => job.RunDailyReconciliationAsync(DateTime.UtcNow.Date.AddDays(-1)),
        Cron.Daily(hour: 2));        // 2 AM UTC every day

    RecurringJob.AddOrUpdate<OverdueBillsJob>(
        "mark-overdue-bills",
        job => job.MarkOverdueBillsAsync(),
        Cron.Daily(hour: 0, minute: 5)); // 00:05 UTC every day
}

app.Run();
