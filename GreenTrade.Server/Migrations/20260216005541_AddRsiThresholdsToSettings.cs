using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenTrade.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRsiThresholdsToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RsiOverboughtThreshold",
                table: "MarketSettings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RsiOversoldThreshold",
                table: "MarketSettings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RsiOverboughtThreshold",
                table: "MarketSettings");

            migrationBuilder.DropColumn(
                name: "RsiOversoldThreshold",
                table: "MarketSettings");
        }
    }
}
