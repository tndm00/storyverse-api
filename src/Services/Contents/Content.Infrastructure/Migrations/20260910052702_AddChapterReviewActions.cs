using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChapterReviewActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chapter_review_actions",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChapterId = table.Column<long>(type: "bigint", nullable: false),
                    ModeratorUserId = table.Column<long>(type: "bigint", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chapter_review_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chapter_review_actions_chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalSchema: "content",
                        principalTable: "chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chapter_review_actions_ChapterId",
                schema: "content",
                table: "chapter_review_actions",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_chapter_review_actions_ModeratorUserId",
                schema: "content",
                table: "chapter_review_actions",
                column: "ModeratorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_chapter_review_actions_PublicId",
                schema: "content",
                table: "chapter_review_actions",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chapter_review_actions",
                schema: "content");
        }
    }
}
