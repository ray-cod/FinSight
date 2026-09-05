using FinSight.Application.Abstractions.AI;
using FinSight.Application.Abstractions.Banking;
using FinSight.Application.Abstractions.Caching;
using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Intelligence;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Notifications;
using FinSight.Application.Abstractions.Observability;
using FinSight.Application.Abstractions.Outbox;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Abstractions.Security;
using FinSight.Application.Features.Transactions;
using FinSight.Infrastructure.AI.OpenAI;
using FinSight.Infrastructure.AI.Rules;
using FinSight.Infrastructure.Audit;
using FinSight.Infrastructure.Banking.MockBank;
using FinSight.Infrastructure.Caching.Redis;
using FinSight.Infrastructure.Configuration;
using FinSight.Infrastructure.Health;
using FinSight.Infrastructure.Identity;
using FinSight.Infrastructure.Intelligence;
using FinSight.Infrastructure.Messaging.RabbitMq;
using FinSight.Infrastructure.Notifications;
using FinSight.Infrastructure.Observability;
using FinSight.Infrastructure.Outbox;
using FinSight.Infrastructure.Persistence;
using FinSight.Infrastructure.Persistence.Repositories;
using FinSight.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    /// <param name="environment">
    /// The host environment used to select the appropriate notification sender.
    /// </param>
    /// <param name="configureAuthentication">
    /// A boolean indicating whether to configure JWT authentication. Defaults to true.
    /// </param>
    /// <param name="configureIdentity">
    /// A boolean indicating whether to configure ASP.NET Core Identity. Defaults to true.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> instance.
    /// </returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        bool configureAuthentication = true,
        bool configureIdentity = true)
    {
        // SMTP should only be validated in production;
        // in development we use DevelopmentNotificationSender which doesn't need SMTP configuration
        bool configureSmtp = !environment.IsDevelopment();

        AddOptions(
            services, configureAuthentication, configureSmtp);

        AddDatabase(
            services);

        AddRedis(
            services);

        AddRabbitMq(
            services);

        if (configureIdentity)
        {
            AddIdentity(
                services);
        }

        if (configureAuthentication)
        {
            services.AddJwtAuthentication(
                configuration);
        }

        AddBanking(services);

        AddRepositories(services);

        if (environment.IsDevelopment())
        {
            services.AddSingleton<INotificationSender, DevelopmentNotificationSender>();
        }
        else
        {
            services.AddSingleton<INotificationSender, SmtpNotificationSender>();
        }

        services.AddScoped<FinancialSeedService>();

        // Register the concrete RabbitMQ publisher so it can be resolved
        // directly. Avoid registering it as the default `IEventPublisher`
        // here to prevent accidental overrides when a different
        // `IEventPublisher` (for example, an outbox-based publisher)
        // should be used by higher-level components.
        services.AddSingleton<RabbitMqEventPublisher>();

        AddAi(services);

        if (!configureIdentity)
        {
            // When identity is not configured (e.g., in background workers),
            // use a null implementation to satisfy dependency injection.
            services.AddScoped<IUserContactService, NullUserContactService>();
        }

        // Telemetry adapter
        services.AddSingleton<IFinSightTelemetry, FinSightTelemetryAdapter>();

        services.AddScoped<TransactionProcessingService>();

        services.AddScoped<MerchantResolutionService>();

        services.AddScoped<CategorySeedService>();

        services.AddScoped<IAnomalyDetector, AnomalyDetector>();

        services.AddSingleton<IInsightGenerator, InsightGenerator>();

        // Use the outbox publisher as the default `IEventPublisher` for
        // request/transactional flows (API). Background workers can
        // override this registration to publish directly to RabbitMQ.
        services.AddScoped<IEventPublisher, OutboxEventPublisher>();

        services.AddSingleton<ReliableRabbitMqPublisher>();

        services.AddHostedService<OutboxDispatcher>();

        AddHealthChecks(services);

        return services;
    }

    private static void AddOptions(
        IServiceCollection services, bool configureAuthentication, bool configureSmtp = true)
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

        if (configureSmtp)
        {
            services
                .AddOptions<SmtpOptions>()
                .BindConfiguration(
                    SmtpOptions.SectionName)
                .Validate(
                    options =>
                        !string.IsNullOrWhiteSpace(
                            options.Host),
                    "SMTP host is required.")
                .Validate(
                    options =>
                        !string.IsNullOrWhiteSpace(
                            options.FromAddress),
                    "SMTP sender address is required.")
                .ValidateOnStart();
        }
        else
        {
            // In development, bind SMTP options but don't validate
            services
                .AddOptions<SmtpOptions>()
                .BindConfiguration(
                    SmtpOptions.SectionName);
        }

        if (configureAuthentication)
        {
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
                        options.SigningKey is not null &&
                        options.SigningKey.Length >= 64,
                    "JWT signing key must contain at least 64 characters.")
                .ValidateOnStart();
        }
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
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay:
                                TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);

                        npgsqlOptions.CommandTimeout(30);

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

        services.AddScoped<
            IEventPublisher,
            RabbitMqEventPublisher>();

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

        services.AddHttpContextAccessor();

        services.AddScoped<IAuditService, PersistentAuditService>();

        services.AddScoped<IUserContactService, UserContactService>();

        services.AddScoped<IdentitySeedService>();
    }

    private static void AddBanking(
        IServiceCollection services)
    {
        services.AddScoped<
            IBankProvider,
            MockBankProvider>();

        services.AddScoped<
            IBankTransactionProvider,
            MockBankTransactionProvider>();
    }

    private static void AddRepositories(
        IServiceCollection services)
    {
        services.AddScoped<
            IInstitutionRepository,
            InstitutionRepository>();

        services.AddScoped<
            IAccountConnectionRepository,
            AccountConnectionRepository>();

        services.AddScoped<
            IFinancialAccountRepository,
            FinancialAccountRepository>();

        services.AddScoped<
            ITransactionRepository,
            TransactionRepository>();

        services.AddScoped<
            ISubscriptionRepository,
            SubscriptionRepository>();

        services.AddScoped<
            IMerchantRepository,
            MerchantRepository>();

        services.AddScoped<
            ICategoryRepository,
            CategoryRepository>();

        services.AddScoped<
            IAnomalyRepository,
            AnomalyRepository>();

        services.AddScoped<
            IInsightRepository,
            InsightRepository>();

        services.AddScoped<
            IOutboxRepository,
            OutboxRepository>();

        services.AddScoped<
            INotificationRepository,
            NotificationRepository>();

        services.AddScoped<
            INotificationPreferenceRepository,
            NotificationPreferenceRepository>();

        services.AddScoped<
            IProcessedMessageStore,
            ProcessedMessageStore>();
    }

    private static void AddAi(
        IServiceCollection services)
    {
        services
            .AddOptions<OpenAiOptions>()
            .BindConfiguration(
                OpenAiOptions.SectionName)
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ApiKey),
                "OpenAI API key is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Model),
                "OpenAI model is required.")
            .ValidateOnStart();

        services.AddSingleton<
            ITransactionCategorizer,
            OpenAiTransactionCategorizer>();

        services.AddSingleton<
            IMerchantNormalizer,
            MerchantNormalizer>();

        services.AddSingleton<
            ICategoryRuleEngine,
            MerchantCategoryRuleEngine>();
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
