using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class changed_ApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndHoursEarlier",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndHoursFinal",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndHoursLater",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartHoursEarlier",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartHoursFinal",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartHoursLater",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndHoursEarlier",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EndHoursFinal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EndHoursLater",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StartHoursEarlier",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StartHoursFinal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StartHoursLater",
                table: "AspNetUsers");
        }
    }
}
