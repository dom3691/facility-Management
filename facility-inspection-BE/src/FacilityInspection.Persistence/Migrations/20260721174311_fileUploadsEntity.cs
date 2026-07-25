using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilityInspection.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fileUploadsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetTag",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Incidents");

            migrationBuilder.RenameColumn(
                name: "ReportedDate",
                table: "Incidents",
                newName: "IncidentDate");

            migrationBuilder.AddColumn<string>(
                name: "BusinessUnit",
                table: "Incidents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityId",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "IncidentNumber",
                table: "Incidents",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SAPId",
                table: "Incidents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_FacilityId",
                table: "Incidents",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentNumber",
                table: "Incidents",
                column: "IncidentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_LocationId",
                table: "Incidents",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Facilities_FacilityId",
                table: "Incidents",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Locations_LocationId",
                table: "Incidents",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Facilities_FacilityId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Locations_LocationId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_FacilityId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_IncidentNumber",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_LocationId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "BusinessUnit",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IncidentNumber",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "SAPId",
                table: "Incidents");

            migrationBuilder.RenameColumn(
                name: "IncidentDate",
                table: "Incidents",
                newName: "ReportedDate");

            migrationBuilder.AddColumn<string>(
                name: "AssetTag",
                table: "Incidents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Incidents",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Incidents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
