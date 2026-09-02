using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Accounts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinSight.Workers.Workers;

/// <summary>
/// Periodically synchronizes active financial institution connections.
/// </summary>
public sealed partial class BankSyncWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<BankSyncWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromMinutes(15);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        LogWorkerStarted();

        await SynchronizeActiveConnectionsAsync(
            stoppingToken);

        using var timer =
            new PeriodicTimer(Interval);

        while (
            await timer.WaitForNextTickAsync(
                stoppingToken))
        {
            await SynchronizeActiveConnectionsAsync(
                stoppingToken);
        }
    }

    private async Task SynchronizeActiveConnectionsAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            scopeFactory.CreateScope();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<
                    IAccountConnectionRepository>();

        var syncService =
            scope.ServiceProvider
                .GetRequiredService<
                    AccountSyncService>();

        var connections =
            await repository.GetAllActiveAsync(
                cancellationToken);

        foreach (var connection in connections)
        {
            try
            {
                await syncService.SyncAsync(
                    connection.UserId,
                    connection.Id,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                LogSyncFailed(
                    exception,
                    connection.Id);
            }
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Bank synchronization worker started.")]
    private partial void LogWorkerStarted();

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Scheduled synchronization failed for connection {ConnectionId}.")]
    private partial void LogSyncFailed(Exception exception, Guid connectionId);
}
