using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Tags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkTaskTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Projects_IsArchived",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "WorkTasks",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "WorkTasks");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTaskTags",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTaskTags", x => new { x.TaskId, x.TagId });
                    table.ForeignKey(
                        name: "FK_WorkTaskTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTaskTags_WorkTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsArchived",
                table: "Projects",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTaskTags_TagId",
                table: "WorkTaskTags",
                column: "TagId");
        }
    }
}
