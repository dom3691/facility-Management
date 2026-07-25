using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilityInspection.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addedVendor_and_workOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "StartedDate",
                table: "WorkOrders");

            migrationBuilder.RenameColumn(
                name: "ScopeOfWork",
                table: "WorkOrders",
                newName: "Description");

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "VendorAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "VendorCategory",
                table: "VendorAssignments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_IncidentId",
                table: "WorkOrders",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_VendorId",
                table: "WorkOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorAssignments_IncidentId",
                table: "VendorAssignments",
                column: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorAssignments_Incidents_IncidentId",
                table: "VendorAssignments",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Incidents_IncidentId",
                table: "WorkOrders",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Vendors_VendorId",
                table: "WorkOrders",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorAssignments_Incidents_IncidentId",
                table: "VendorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Incidents_IncidentId",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Vendors_VendorId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_IncidentId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_VendorId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_VendorAssignments_IncidentId",
                table: "VendorAssignments");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "VendorAssignments");

            migrationBuilder.DropColumn(
                name: "VendorCategory",
                table: "VendorAssignments");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "WorkOrders",
                newName: "ScopeOfWork");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ScheduledDate",
                table: "WorkOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedDate",
                table: "WorkOrders",
                type: "datetimeoffset",
                nullable: true);
        }
    }
}
