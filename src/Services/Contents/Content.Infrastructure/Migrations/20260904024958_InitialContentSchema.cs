using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialContentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "genres",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stories",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthorProfileId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginalSource = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AgeRating = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    FollowCount = table.Column<int>(type: "integer", nullable: false),
                    RatingAvg = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    RatingCount = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "story_genres",
                schema: "content",
                columns: table => new
                {
                    StoryId = table.Column<long>(type: "bigint", nullable: false),
                    GenreId = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_story_genres", x => new { x.StoryId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_story_genres_genres_GenreId",
                        column: x => x.GenreId,
                        principalSchema: "content",
                        principalTable: "genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_story_genres_stories_StoryId",
                        column: x => x.StoryId,
                        principalSchema: "content",
                        principalTable: "stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "volumes",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoryId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_volumes_stories_StoryId",
                        column: x => x.StoryId,
                        principalSchema: "content",
                        principalTable: "stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "story_tags",
                schema: "content",
                columns: table => new
                {
                    StoryId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_story_tags", x => new { x.StoryId, x.TagId });
                    table.ForeignKey(
                        name: "FK_story_tags_stories_StoryId",
                        column: x => x.StoryId,
                        principalSchema: "content",
                        principalTable: "stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_story_tags_tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "content",
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chapters",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoryId = table.Column<long>(type: "bigint", nullable: false),
                    VolumeId = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    WordCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AccessType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PriceCoin = table.Column<int>(type: "integer", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chapters_stories_StoryId",
                        column: x => x.StoryId,
                        principalSchema: "content",
                        principalTable: "stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chapters_volumes_VolumeId",
                        column: x => x.VolumeId,
                        principalSchema: "content",
                        principalTable: "volumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "content",
                table: "genres",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "IsActive", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, "Ngôn Tình", "ngon-tinh", null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, "Kiếm Hiệp", "kiem-hiep", null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, true, "Tiên Hiệp", "tien-hiep", null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, "Huyền Huyễn", "huyen-huyen", null },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, "Đô Thị", "do-thi", null },
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, "Trinh Thám", "trinh-tham", null },
                    { 7L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, "Kinh Dị", "kinh-di", null },
                    { 8L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, "Hài Hước", "hai-huoc", null },
                    { 9L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 9, true, "Hành Động", "hanh-dong", null },
                    { 10L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10, true, "Xuyên Không", "xuyen-khong", null },
                    { 11L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 11, true, "Trọng Sinh", "trong-sinh", null },
                    { 12L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 12, true, "Dị Giới", "di-gioi", null },
                    { 13L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 13, true, "Khoa Huyễn", "khoa-huyen", null },
                    { 14L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 14, true, "Quân Sự", "quan-su", null },
                    { 15L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 15, true, "Lịch Sử", "lich-su", null },
                    { 16L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 16, true, "Đồng Nhân", "dong-nhan", null },
                    { 17L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 17, true, "Light Novel", "light-novel", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_chapters_PublicId",
                schema: "content",
                table: "chapters",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chapters_Status_ScheduledAt",
                schema: "content",
                table: "chapters",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_chapters_StoryId_OrderIndex",
                schema: "content",
                table: "chapters",
                columns: new[] { "StoryId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_chapters_VolumeId",
                schema: "content",
                table: "chapters",
                column: "VolumeId");

            migrationBuilder.CreateIndex(
                name: "IX_genres_Name",
                schema: "content",
                table: "genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_genres_Slug",
                schema: "content",
                table: "genres",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stories_AuthorProfileId",
                schema: "content",
                table: "stories",
                column: "AuthorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_stories_PublicId",
                schema: "content",
                table: "stories",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stories_Slug",
                schema: "content",
                table: "stories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stories_Status",
                schema: "content",
                table: "stories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_story_genres_GenreId",
                schema: "content",
                table: "story_genres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_story_genres_StoryId",
                schema: "content",
                table: "story_genres",
                column: "StoryId",
                unique: true,
                filter: "\"IsPrimary\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_story_tags_TagId",
                schema: "content",
                table: "story_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_Name",
                schema: "content",
                table: "tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_Slug",
                schema: "content",
                table: "tags",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_UsageCount",
                schema: "content",
                table: "tags",
                column: "UsageCount");

            migrationBuilder.CreateIndex(
                name: "IX_volumes_PublicId",
                schema: "content",
                table: "volumes",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_volumes_StoryId_OrderIndex",
                schema: "content",
                table: "volumes",
                columns: new[] { "StoryId", "OrderIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chapters",
                schema: "content");

            migrationBuilder.DropTable(
                name: "story_genres",
                schema: "content");

            migrationBuilder.DropTable(
                name: "story_tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "volumes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "genres",
                schema: "content");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "stories",
                schema: "content");
        }
    }
}
