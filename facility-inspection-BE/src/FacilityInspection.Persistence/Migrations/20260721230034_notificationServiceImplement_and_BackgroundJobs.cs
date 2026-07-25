using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilityInspection.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class notificationServiceImplement_and_BackgroundJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientUserId_Status",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "RelatedEntityType",
                table: "Notifications",
                newName: "RelatedEntityName");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Notifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                table: "Notifications",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                table: "Notifications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientUserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Status",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "RelatedEntityName",
                table: "Notifications",
                newName: "RelatedEntityType");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_Status",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "Status" });
        }
    }
}
