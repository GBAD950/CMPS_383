using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SP22.P03.Web.Migrations
{
    public partial class AddingSaleEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaleEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleEventProduct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleEventPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SaleEventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleEventProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleEventProduct_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleEventProduct_SaleEvent_SaleEventId",
                        column: x => x.SaleEventId,
                        principalTable: "SaleEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleEventProduct_ProductId",
                table: "SaleEventProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleEventProduct_SaleEventId",
                table: "SaleEventProduct",
                column: "SaleEventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleEventProduct");

            migrationBuilder.DropTable(
                name: "SaleEvent");
        }
    }
}
