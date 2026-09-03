using FinSight.Infrastructure.AI.Rules;
using FluentAssertions;

namespace FinSight.Application.Tests.TransactionRules;

/// <summary>
/// Tests deterministic merchant classification rules.
/// </summary>
public sealed class MerchantCategoryRuleEngineTests
{
    private readonly MerchantCategoryRuleEngine _engine = new();

    /// <summary>
    /// Verifies that Amazon is classified without AI.
    /// </summary>
    [Fact]
    public void AmazonShouldClassifyAsShopping()
    {
        var result =
            _engine.TryClassify(
                "AMZN MKTP US");

        result.Should().NotBeNull();

        result!.Merchant
            .Should()
            .Be("Amazon");

        result.CategoryCode
            .Should()
            .Be("SHOPPING");

        result.SubcategoryCode
            .Should()
            .Be("ONLINE_SHOPPING");
    }

    /// <summary>
    /// Verifies that unknown descriptions do not match deterministic rules.
    /// </summary>
    [Fact]
    public void UnknownDescriptionShouldNotMatch()
    {
        var result =
            _engine.TryClassify(
                "COMPLETELY UNKNOWN MERCHANT");

        result.Should().BeNull();
    }
}
