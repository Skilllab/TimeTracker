using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_WorkTasks_TaskId",
                table: "TimeEntries");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "WorkTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_IsDeleted",
                table: "WorkTasks",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_WorkTasks_TaskId",
                table: "TimeEntries",
                column: "TaskId",
                principalTable: "WorkTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_WorkTasks_TaskId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_WorkTasks_IsDeleted",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WorkTasks");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_WorkTasks_TaskId",
                table: "TimeEntries",
                column: "TaskId",
                principalTable: "WorkTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
