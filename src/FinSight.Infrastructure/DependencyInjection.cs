using FinSight.Application.Abstractions.Caching;
using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Abstractions.Security;
using FinSight.Infrastructure.Audit;
using FinSight.Infrastructure.Caching.Redis;
using FinSight.Infrastructure.Configuration;
using FinSight.Infrastructure.Health;
using FinSight.Infrastructure.Identity;
using FinSight.Infrastructure.Messaging.RabbitMq;
using FinSight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FinSight.Infrastructure;

/// <summary>
/// Provides extension methods for configuring infrastructure services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services including persistence, Redis caching, RabbitMQ messaging,
    /// ASP.NET Core Identity, JWT authentication, options validation, and health checks.
    /// </summary>
    /// <param name="services">
    /// The service collection to add infrastructure dependencies to.
    /// </param>
    /// <param name="configuration">
    /// The application configuration.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> instance.
    /// </returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddOptions(
            services);

        AddDatabase(
            services);

        AddRedis(
            services);

        AddRabbitMq(
            services);

        AddIdentity(
            services);

        services.AddJwtAuthentication(
            configuration);

        AddHealthChecks(
            services);

        return services;
    }

    private static void AddOptions(
        IServiceCollection services)
    {
        services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(
                DatabaseOptions.SectionName)
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ConnectionString),
                "Database connection string is required.")
            .ValidateOnStart();

        services
            .AddOptions<RedisOptions>()
            .BindConfiguration(
                RedisOptions.SectionName)
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ConnectionString),
                "Redis connection string is required.")
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqOptions>()
            .BindConfiguration(
                RabbitMqOptions.SectionName)
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Host),
                "RabbitMQ host is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Username),
                "RabbitMQ username is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Password),
                "RabbitMQ password is required.")
            .ValidateOnStart();

        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(
                JwtOptions.SectionName)
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Issuer),
                "JWT issuer is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Audience),
                "JWT audience is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.SigningKey),
                "JWT signing key is required.")
            .Validate(
                options =>
                    options.SigningKey.Length >= 64,
                "JWT signing key must contain at least 64 characters.")
            .ValidateOnStart();
    }

    private static void AddDatabase(
        IServiceCollection services)
    {
        services.AddDbContext<FinSightDbContext>(
            (serviceProvider, options) =>
            {
                var databaseOptions =
                    serviceProvider
                        .GetRequiredService<
                            IOptions<DatabaseOptions>>()
                        .Value;

                options.UseNpgsql(
                    databaseOptions.ConnectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(
                            typeof(FinSightDbContext).Assembly
                                .GetName()
                                .Name);
                    });
            });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
    }

    private static void AddRedis(
        IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(
            serviceProvider =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<
                            IOptions<RedisOptions>>()
                        .Value;

                return ConnectionMultiplexer.Connect(
                    options.ConnectionString);
            });

        services.AddSingleton<ICacheService, RedisCacheService>();
    }

    private static void AddRabbitMq(
        IServiceCollection services)
    {
        services.AddSingleton<
            RabbitMqConnectionProvider>();

        services.AddSingleton<
            IRabbitMqConnectionProvider>(
            serviceProvider =>
                serviceProvider
                    .GetRequiredService<
                        RabbitMqConnectionProvider>());

        services.AddHostedService<
            RabbitMqTopologyHostedService>();
    }

    private static void AddIdentity(
        IServiceCollection services)
    {
        services
            .AddIdentityCore<ApplicationUser>(
                IdentityConfiguration.Configure)
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<FinSightDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthService, IdentityService>();

        services.AddScoped<IUserService, UserService>();

        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IPasswordResetService, PasswordResetService>();

        services.AddScoped<IAuditService, AuditService>();

        services.AddScoped<IdentitySeedService>();
    }

    private static void AddHealthChecks(
        IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<PostgresHealthCheck>(
                "postgres",
                tags: ["ready"])
            .AddCheck<RedisHealthCheck>(
                "redis",
                tags: ["ready"])
            .AddCheck<RabbitMqHealthCheck>(
                "rabbitmq",
                tags: ["ready"]);
    }
}
