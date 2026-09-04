using FinSight.Domain.Subscriptions;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations for detected subscriptions.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Gets all subscriptions belonging to a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="includeDismissed">
    /// Whether dismissed subscriptions should be included.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's subscriptions.</returns>
    Task<IReadOnlyList<Subscription>> GetByUserIdAsync(
        Guid userId,
        bool includeDismissed = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subscription within a user ownership scope.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscription when found.</returns>
    Task<Subscription?> GetByIdAsync(
        Guid userId,
        Guid subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a subscription for a merchant and currency within a user scope.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="merchantId">The merchant identifier.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching subscription.</returns>
    Task<Subscription?> GetByMerchantAsync(
        Guid userId,
        Guid merchantId,
        string currency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active subscriptions whose expected charge is sufficiently overdue.
    /// </summary>
    /// <param name="asOf">The evaluation time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Potentially inactive subscriptions.</returns>
    Task<IReadOnlyList<Subscription>> GetOverdueAsync(
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a transaction has already been recorded as a subscription price observation.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the observation exists.</returns>
    Task<bool> HasPriceObservationAsync(
        Guid subscriptionId,
        Guid transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a subscription.
    /// </summary>
    /// <param name="subscription">The subscription.</param>
    void Add(Subscription subscription);

    /// <summary>
    /// Adds a subscription price observation.
    /// </summary>
    /// <param name="history">The price observation.</param>
    void AddPriceHistory(
        SubscriptionPriceHistory history);

    /// <summary>
    /// Gets the recent subscription price observations.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <param name="limit">Maximum observations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recent price observations.</returns>
    Task<IReadOnlyList<SubscriptionPriceHistory>>
        GetPriceHistoryAsync(
            Guid subscriptionId,
            int limit = 24,
            CancellationToken cancellationToken = default);
}
