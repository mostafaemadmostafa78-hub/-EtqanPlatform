using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactTableToMatchDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReviewId",
                table: "ContactMessages",
                newName: "CompanyId");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ArtisanId",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "ContactMessages",
                newName: "ReviewId");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ArtisanId",
                table: "ContactMessages",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
