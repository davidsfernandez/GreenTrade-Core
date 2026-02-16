using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenTrade.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddLastModifiedToOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastModifiedById",
                table: "Offers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "Offers");
        }
    }
}
