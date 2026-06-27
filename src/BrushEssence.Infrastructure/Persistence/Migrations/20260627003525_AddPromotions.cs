using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrushEssence.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promotions_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotion_paintings",
                columns: table => new
                {
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaintingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_paintings", x => new { x.PromotionId, x.PaintingId });
                    table.ForeignKey(
                        name: "FK_promotion_paintings_paintings_PaintingId",
                        column: x => x.PaintingId,
                        principalTable: "paintings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promotion_paintings_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_promotion_paintings_PaintingId",
                table: "promotion_paintings",
                column: "PaintingId");

            migrationBuilder.CreateIndex(
                name: "IX_promotions_CategoryId",
                table: "promotions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_promotions_IsActive",
                table: "promotions",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promotion_paintings");

            migrationBuilder.DropTable(
                name: "promotions");
        }
    }
}
