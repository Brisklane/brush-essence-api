using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrushEssence.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetStoreCurrencyToPkr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The store now operates in a single fixed currency (PKR). Convert
            // every existing record so prices, carts, orders and commissions are
            // all expressed consistently.
            migrationBuilder.Sql("UPDATE paintings SET \"Currency\" = 'PKR';");
            migrationBuilder.Sql("UPDATE orders SET \"Currency\" = 'PKR';");
            migrationBuilder.Sql("UPDATE custom_requests SET \"Currency\" = 'PKR';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort revert to the previous default.
            migrationBuilder.Sql("UPDATE paintings SET \"Currency\" = 'USD';");
            migrationBuilder.Sql("UPDATE orders SET \"Currency\" = 'USD';");
            migrationBuilder.Sql("UPDATE custom_requests SET \"Currency\" = 'USD';");
        }
    }
}
