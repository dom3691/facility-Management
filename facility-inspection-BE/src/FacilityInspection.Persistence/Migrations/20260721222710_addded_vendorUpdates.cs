using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilityInspection.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addded_vendorUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "VendorUpdates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "CompletionComment",
                table: "VendorUpdates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompletionUpdate",
                table: "VendorUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionComment",
                table: "VendorUpdates");

            migrationBuilder.DropColumn(
                name: "IsCompletionUpdate",
                table: "VendorUpdates");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "VendorUpdates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
