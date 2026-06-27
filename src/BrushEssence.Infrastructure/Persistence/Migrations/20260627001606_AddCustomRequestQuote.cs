using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrushEssence.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomRequestQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BudgetAmount",
                table: "custom_requests",
                newName: "QuoteAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuoteAmount",
                table: "custom_requests",
                newName: "BudgetAmount");
        }
    }
}
