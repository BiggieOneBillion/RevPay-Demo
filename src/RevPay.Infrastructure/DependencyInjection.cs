using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using RevPay.Application.Common.Interfaces;
using RevPay.Infrastructure.Auth;
using RevPay.Infrastructure.Jobs;
using RevPay.Infrastructure.Ledger;
using RevPay.Infrastructure.Notifications;
using RevPay.Infrastructure.Payments;
using RevPay.Infrastructure.Payments.Gateways;
using RevPay.Infrastructure.Persistence;
using RevPay.Infrastructure.Persistence.Repositories;
using RevPay.Infrastructure.Services;
using StackExchange.Redis;
using Hangfire;
using Hangfire.PostgreSql;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RevPay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        // ── Database ───────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ── Repositories ───────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITaxpayerRepository, TaxpayerRepository>();
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // ── Domain Services ────────────────────────────────────────────────────
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<IReceiptService, ReceiptService>();

        // ── JWT Auth ───────────────────────────────────────────────────────────
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.AddScoped<IJwtService>(_ => new JwtService(
            jwtSettings["SecretKey"] ?? "super_secret_key_that_is_at_least_32_characters_long",
            jwtSettings["Issuer"] ?? "revpay.lagosstate.gov.ng",
            jwtSettings["Audience"] ?? "revpay-clients"
        ));

        // ── Notifications ──────────────────────────────────────────────────────
        services.AddScoped<NotificationService>();
        services.AddScoped<INotificationService>(sp => sp.GetRequiredService<NotificationService>());
        services.AddScoped<IEmailService>(sp => sp.GetRequiredService<NotificationService>());
        services.AddScoped<ISmsService>(sp => sp.GetRequiredService<NotificationService>());

        // ── Polly retry policy shared across gateways ──────────────────────────
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .OrTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        var circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .OrTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

        // ── Payment Gateways ───────────────────────────────────────────────────
        var gatewaySettings = configuration.GetSection("GatewaySettings");

        services.AddHttpClient<PaystackGateway>(client =>
        {
            client.BaseAddress = new Uri("https://api.paystack.co/");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    gatewaySettings["PaystackSecretKey"] ?? "test_paystack_key");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddPolicyHandler(combinedPolicy);

        services.AddHttpClient<FlutterwaveGateway>(client =>
        {
            client.BaseAddress = new Uri("https://api.flutterwave.com/v3/");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    gatewaySettings["FlutterwaveSecretKey"] ?? "test_flw_key");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddPolicyHandler(combinedPolicy);

        // Register both as IPaymentGateway for the factory
        services.AddScoped<IPaymentGateway, PaystackGateway>();
        services.AddScoped<IPaymentGateway, FlutterwaveGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IWebhookValidator, WebhookValidator>();

        // ── Redis  ─────────────────────────────────────────────────────────────
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // ── Hangfire ───────────────────────────────────────────────────────────
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(connectionString));

        services.AddHangfireServer();
        services.AddScoped<ReconciliationJob>();
        services.AddScoped<OverdueBillsJob>();

        return services;
    }
}
