using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialModerationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "moderation");

            migrationBuilder.CreateTable(
                name: "reports",
                schema: "moderation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterUserId = table.Column<long>(type: "bigint", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "moderation_actions",
                schema: "moderation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<long>(type: "bigint", nullable: true),
                    ModeratorUserId = table.Column<long>(type: "bigint", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moderation_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_moderation_actions_reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "moderation",
                        principalTable: "reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_moderation_actions_ModeratorUserId",
                schema: "moderation",
                table: "moderation_actions",
                column: "ModeratorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_moderation_actions_PublicId",
                schema: "moderation",
                table: "moderation_actions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moderation_actions_ReportId",
                schema: "moderation",
                table: "moderation_actions",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_moderation_actions_TargetType_TargetId",
                schema: "moderation",
                table: "moderation_actions",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_reports_PublicId",
                schema: "moderation",
                table: "reports",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reports_ReporterUserId",
                schema: "moderation",
                table: "reports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_reports_Status_CreatedAt",
                schema: "moderation",
                table: "reports",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_reports_TargetType_TargetId",
                schema: "moderation",
                table: "reports",
                columns: new[] { "TargetType", "TargetId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "moderation_actions",
                schema: "moderation");

            migrationBuilder.DropTable(
                name: "reports",
                schema: "moderation");
        }
    }
}
