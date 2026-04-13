using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETQAN_BY_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAudio",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsAudio",
                table: "ChatMessages");
        }
    }
}
