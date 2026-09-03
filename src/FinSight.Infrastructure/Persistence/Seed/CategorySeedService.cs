using FinSight.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds FinSight's initial transaction classification taxonomy.
/// </summary>
public sealed class CategorySeedService(
    FinSightDbContext dbContext)
{
    /// <summary>
    /// Seeds the initial categories and subcategories.
    /// </summary>
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Set<Category>()
            .AnyAsync(cancellationToken))
        {
            return;
        }

        var housing =
            Category.Create(
                "HOUSING",
                "Housing",
                CategoryType.Expense);

        var food =
            Category.Create(
                "FOOD_DINING",
                "Food & Dining",
                CategoryType.Expense);

        var transport =
            Category.Create(
                "TRANSPORTATION",
                "Transportation",
                CategoryType.Expense);

        var shopping =
            Category.Create(
                "SHOPPING",
                "Shopping",
                CategoryType.Expense);

        var entertainment =
            Category.Create(
                "ENTERTAINMENT",
                "Entertainment",
                CategoryType.Expense);

        var healthcare =
            Category.Create(
                "HEALTHCARE",
                "Healthcare",
                CategoryType.Expense);

        var utilities =
            Category.Create(
                "UTILITIES",
                "Utilities",
                CategoryType.Expense);

        var travel =
            Category.Create(
                "TRAVEL",
                "Travel",
                CategoryType.Expense);

        var education =
            Category.Create(
                "EDUCATION",
                "Education",
                CategoryType.Expense);

        var income =
            Category.Create(
                "INCOME",
                "Income",
                CategoryType.Income);

        var transfers =
            Category.Create(
                "TRANSFERS",
                "Transfers",
                CategoryType.Transfer);

        var fees =
            Category.Create(
                "FEES",
                "Fees",
                CategoryType.Fee);

        var other =
            Category.Create(
                "OTHER",
                "Other",
                CategoryType.Other);

        dbContext.Set<Category>().AddRange(
            housing,
            food,
            transport,
            shopping,
            entertainment,
            healthcare,
            utilities,
            travel,
            education,
            income,
            transfers,
            fees,
            other);

        dbContext.Set<Subcategory>().AddRange(
            Subcategory.Create(
                food.Id,
                "COFFEE",
                "Coffee"),

            Subcategory.Create(
                food.Id,
                "GROCERIES",
                "Groceries"),

            Subcategory.Create(
                food.Id,
                "RESTAURANTS",
                "Restaurants"),

            Subcategory.Create(
                transport.Id,
                "RIDESHARE",
                "Rideshare"),

            Subcategory.Create(
                transport.Id,
                "PUBLIC_TRANSPORT",
                "Public Transport"),

            Subcategory.Create(
                shopping.Id,
                "ONLINE_SHOPPING",
                "Online Shopping"),

            Subcategory.Create(
                shopping.Id,
                "RETAIL",
                "Retail"),

            Subcategory.Create(
                entertainment.Id,
                "STREAMING",
                "Streaming"),

            Subcategory.Create(
                entertainment.Id,
                "GAMES",
                "Games"),

            Subcategory.Create(
                travel.Id,
                "FLIGHTS",
                "Flights"),

            Subcategory.Create(
                travel.Id,
                "HOTELS",
                "Hotels"),

            Subcategory.Create(
                utilities.Id,
                "ELECTRICITY",
                "Electricity"),

            Subcategory.Create(
                utilities.Id,
                "INTERNET",
                "Internet"));

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
