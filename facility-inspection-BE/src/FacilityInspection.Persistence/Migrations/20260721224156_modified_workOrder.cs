using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilityInspection.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class modified_workOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Verifications_WorkOrderId",
                table: "Verifications");

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_WorkOrderId",
                table: "Verifications",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Verifications_WorkOrderId",
                table: "Verifications");

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_WorkOrderId",
                table: "Verifications",
                column: "WorkOrderId",
                unique: true);
        }
    }
}
