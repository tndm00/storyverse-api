using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialLibrarySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "library");

            migrationBuilder.CreateTable(
                name: "library_entries",
                schema: "library",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    StoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelfStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_library_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reading_progress",
                schema: "library",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    StoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastChapterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScrollPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LastReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_progress", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_library_entries_PublicId",
                schema: "library",
                table: "library_entries",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_library_entries_UserId_ShelfStatus",
                schema: "library",
                table: "library_entries",
                columns: new[] { "UserId", "ShelfStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_library_entries_UserId_StoryId",
                schema: "library",
                table: "library_entries",
                columns: new[] { "UserId", "StoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reading_progress_PublicId",
                schema: "library",
                table: "reading_progress",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reading_progress_UserId_LastReadAt",
                schema: "library",
                table: "reading_progress",
                columns: new[] { "UserId", "LastReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_reading_progress_UserId_StoryId",
                schema: "library",
                table: "reading_progress",
                columns: new[] { "UserId", "StoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "library_entries",
                schema: "library");

            migrationBuilder.DropTable(
                name: "reading_progress",
                schema: "library");
        }
    }
}
