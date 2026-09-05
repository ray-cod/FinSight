using FinSight.Application.Features.Accounts;
using FinSight.Application.Features.Anomalies;
using FinSight.Application.Features.Insights;
using FinSight.Application.Features.Institutions;
using FinSight.Application.Features.Subscriptions;
using FinSight.Application.Features.Transactions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinSight.Application;

/// <summary>
/// Provides dependency registration for the application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers FinSight application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        services.AddScoped<AccountService>();
        services.AddScoped<AccountSyncService>();
        services.AddScoped<InstitutionService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<SubscriptionDetectionService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<AnomalyDetectionService>();
        services.AddScoped<AnomalyService>();
        services.AddScoped<InsightService>();
        services.AddScoped<SubscriptionPriceInsightService>();

        return services;
    }
}
