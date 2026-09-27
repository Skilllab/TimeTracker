using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkTasksAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TaskId",
                table: "TimeEntries",
                type: "TEXT",
                nullable: true);

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
                name: "WorkTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<string>(type: "TEXT", nullable: true),
                    LastStartedAt = table.Column<string>(type: "TEXT", nullable: true),
                    FinishedAt = table.Column<string>(type: "TEXT", nullable: true),
                    PausedAt = table.Column<string>(type: "TEXT", nullable: true),
                    PausedSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBillable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_TimeEntries_TaskId",
                table: "TimeEntries",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_LastStartedAt",
                table: "WorkTasks",
                column: "LastStartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_ProjectId",
                table: "WorkTasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_Status",
                table: "WorkTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTaskTags_TagId",
                table: "WorkTaskTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_WorkTasks_TaskId",
                table: "TimeEntries",
                column: "TaskId",
                principalTable: "WorkTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_WorkTasks_TaskId",
                table: "TimeEntries");

            migrationBuilder.DropTable(
                name: "WorkTaskTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_TaskId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "TimeEntries");
        }
    }
}
