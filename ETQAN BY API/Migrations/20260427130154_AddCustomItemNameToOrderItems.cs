using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomItemNameToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomItemName",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomItemName",
                table: "OrderItems");
        }
    }
}
