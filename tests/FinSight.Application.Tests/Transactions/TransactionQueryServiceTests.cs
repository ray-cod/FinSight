using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Transactions;
using FluentAssertions;

namespace FinSight.Application.Tests.Transactions;

/// <summary>
/// Tests transaction query orchestration and request normalization.
/// </summary>
public sealed class TransactionQueryServiceTests
{
    /// <summary>
    /// Verifies that pagination and filter bounds are normalized before the repository is queried.
    /// </summary>
    [Fact]
    public async Task GetPageAsyncNormalizesPaginationAndFilters()
    {
        var repository = new StubTransactionQueryRepository();
        var service = new TransactionQueryService(repository);

        await service.GetPageAsync(
            Guid.NewGuid(),
            new TransactionListRequest
            {
                Page = 0,
                PageSize = 999,
                Search = "  coffee  ",
            });

        repository.LastQuery.Should().NotBeNull();
        repository.LastQuery!.Page.Should().Be(1);
        repository.LastQuery.PageSize.Should().Be(100);
        repository.LastQuery.Search.Should().Be("coffee");
    }

    private sealed class StubTransactionQueryRepository
        : ITransactionQueryRepository
    {
        public TransactionQuery? LastQuery { get; private set; }

        public Task<TransactionQueryResult> GetPageAsync(
            Guid userId,
            TransactionQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(
                new TransactionQueryResult(
                    Array.Empty<TransactionListItemResponse>(),
                    query.Page,
                    query.PageSize,
                    0,
                    1));
        }

        public Task<TransactionDetailResponse?> GetDetailAsync(
            Guid userId,
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TransactionDetailResponse?>(null);
        }
    }
}
