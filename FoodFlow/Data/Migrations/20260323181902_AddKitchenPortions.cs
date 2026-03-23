using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKitchenPortions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KitchenPortions",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KitchenPortions",
                table: "MenuItems");
        }
    }
}
