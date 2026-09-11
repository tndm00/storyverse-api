using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChapterReviewActionCascadeOnChapterDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chapter_review_actions_chapters_ChapterId",
                schema: "content",
                table: "chapter_review_actions");

            migrationBuilder.AddForeignKey(
                name: "FK_chapter_review_actions_chapters_ChapterId",
                schema: "content",
                table: "chapter_review_actions",
                column: "ChapterId",
                principalSchema: "content",
                principalTable: "chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chapter_review_actions_chapters_ChapterId",
                schema: "content",
                table: "chapter_review_actions");

            migrationBuilder.AddForeignKey(
                name: "FK_chapter_review_actions_chapters_ChapterId",
                schema: "content",
                table: "chapter_review_actions",
                column: "ChapterId",
                principalSchema: "content",
                principalTable: "chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
