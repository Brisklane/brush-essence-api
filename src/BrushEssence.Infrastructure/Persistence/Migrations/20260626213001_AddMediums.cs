using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrushEssence.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. The new mediums lookup table.
            migrationBuilder.CreateTable(
                name: "mediums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediums", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mediums_Name",
                table: "mediums",
                column: "Name",
                unique: true);

            // 2. New nullable FK column on paintings.
            migrationBuilder.AddColumn<Guid>(
                name: "MediumId",
                table: "paintings",
                type: "uuid",
                nullable: true);

            // 3. Migrate existing free-text mediums into the table, then link
            //    each painting to the matching row (before the old column is dropped).
            migrationBuilder.Sql(@"
                INSERT INTO mediums (""Id"", ""Name"", ""CreatedAt"")
                SELECT gen_random_uuid(), x.medium, now()
                FROM (
                    SELECT DISTINCT btrim(""Medium"") AS medium
                    FROM paintings
                    WHERE ""Medium"" IS NOT NULL AND btrim(""Medium"") <> ''
                ) x;");

            migrationBuilder.Sql(@"
                UPDATE paintings p
                SET ""MediumId"" = m.""Id""
                FROM mediums m
                WHERE btrim(p.""Medium"") = m.""Name"";");

            // Seed a sensible default so the picker is never empty.
            migrationBuilder.Sql(@"
                INSERT INTO mediums (""Id"", ""Name"", ""CreatedAt"")
                SELECT gen_random_uuid(), 'Oil on canvas', now()
                WHERE NOT EXISTS (SELECT 1 FROM mediums WHERE ""Name"" = 'Oil on canvas');");

            // 4. Index + FK for the new column.
            migrationBuilder.CreateIndex(
                name: "IX_paintings_MediumId",
                table: "paintings",
                column: "MediumId");

            migrationBuilder.AddForeignKey(
                name: "FK_paintings_mediums_MediumId",
                table: "paintings",
                column: "MediumId",
                principalTable: "mediums",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // 5. Drop the now-migrated free-text column.
            migrationBuilder.DropColumn(
                name: "Medium",
                table: "paintings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_paintings_mediums_MediumId",
                table: "paintings");

            migrationBuilder.DropTable(
                name: "mediums");

            migrationBuilder.DropIndex(
                name: "IX_paintings_MediumId",
                table: "paintings");

            migrationBuilder.DropColumn(
                name: "MediumId",
                table: "paintings");

            migrationBuilder.AddColumn<string>(
                name: "Medium",
                table: "paintings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
