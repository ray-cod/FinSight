using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinSight.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionIntelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subscription_price_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ObservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_price_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MerchantName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AverageAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DetectionConfidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    FirstDetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastChargeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NextExpectedChargeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastPriceChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_price_history_SubscriptionId_ObservedAt",
                table: "subscription_price_history",
                columns: new[] { "SubscriptionId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_price_history_SubscriptionId_TransactionId",
                table: "subscription_price_history",
                columns: new[] { "SubscriptionId", "TransactionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_NextExpectedChargeAt",
                table: "subscriptions",
                column: "NextExpectedChargeAt");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_MerchantId_Currency",
                table: "subscriptions",
                columns: new[] { "UserId", "MerchantId", "Currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_Status",
                table: "subscriptions",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscription_price_history");

            migrationBuilder.DropTable(
                name: "subscriptions");
        }
    }
}
