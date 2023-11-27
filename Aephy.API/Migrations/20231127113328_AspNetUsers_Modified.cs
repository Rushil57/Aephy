using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class AspNetUsers_Modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "onFriday",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "onMonday",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "onSaturday",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "onSunday",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "onThursday",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "onTuesday",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "onWednesday",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onFriday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "onMonday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "onSaturday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "onSunday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "onThursday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "onTuesday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "onWednesday",
                table: "AspNetUsers");
        }
    }
}
