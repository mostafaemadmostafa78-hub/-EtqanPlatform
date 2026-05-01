using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceRequestAndCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CompanyId",
                table: "ServiceRequests",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Companies_CompanyId",
                table: "ServiceRequests",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Companies_CompanyId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_CompanyId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ServiceRequests");
        }
    }
}
