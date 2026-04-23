using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAgeToBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Artisans");

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDay",
                table: "Artisans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDay",
                table: "Artisans");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Artisans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
