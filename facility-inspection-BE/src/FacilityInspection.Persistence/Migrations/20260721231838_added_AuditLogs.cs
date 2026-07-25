using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilityInspection.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class added_AuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "AuditLogs",
                newName: "PerformedByName");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuditLogs",
                newName: "PerformedByUserId");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "AuditLogs",
                newName: "PerformedDate");

            migrationBuilder.RenameColumn(
                name: "Changes",
                table: "AuditLogs",
                newName: "OldValues");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_PerformedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                newName: "IX_AuditLogs_PerformedDate");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewValues",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "AuditLogs",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "NewValues",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "PerformedDate",
                table: "AuditLogs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "PerformedByUserId",
                table: "AuditLogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PerformedByName",
                table: "AuditLogs",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "OldValues",
                table: "AuditLogs",
                newName: "Changes");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_PerformedDate",
                table: "AuditLogs",
                newName: "IX_AuditLogs_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_PerformedByUserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "AuditLogs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
