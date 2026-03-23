using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseListLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseListId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SuggestedQuantity = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseListLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseListLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseListLines_PurchaseLists_PurchaseListId",
                        column: x => x.PurchaseListId,
                        principalTable: "PurchaseLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseListLines_ProductId",
                table: "PurchaseListLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseListLines_PurchaseListId",
                table: "PurchaseListLines",
                column: "PurchaseListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseListLines");

            migrationBuilder.DropTable(
                name: "PurchaseLists");
        }
    }
}
