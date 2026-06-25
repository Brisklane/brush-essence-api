using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrushEssence.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_paintings_IsPublished",
                table: "paintings");

            migrationBuilder.CreateIndex(
                name: "IX_paintings_IsPublished_CreatedAt",
                table: "paintings",
                columns: new[] { "IsPublished", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_paintings_Price",
                table: "paintings",
                column: "Price");

            // Trigram GIN indexes make case-insensitive substring search
            // (ILIKE '%term%') index-backed instead of a full table scan — the
            // recommended, license-free way to do fuzzy search in PostgreSQL.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(
                "CREATE INDEX \"IX_paintings_Title_trgm\" ON paintings USING gin (\"Title\" gin_trgm_ops);");
            migrationBuilder.Sql(
                "CREATE INDEX \"IX_paintings_Description_trgm\" ON paintings USING gin (\"Description\" gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_paintings_Title_trgm\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_paintings_Description_trgm\";");

            migrationBuilder.DropIndex(
                name: "IX_paintings_IsPublished_CreatedAt",
                table: "paintings");

            migrationBuilder.DropIndex(
                name: "IX_paintings_Price",
                table: "paintings");

            migrationBuilder.CreateIndex(
                name: "IX_paintings_IsPublished",
                table: "paintings",
                column: "IsPublished");
        }
    }
}
