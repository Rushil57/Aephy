using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class FreelancerDetails_Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "FreelancerDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotAvailableForNextSixMonth",
                table: "FreelancerDetails",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWeekendExclude",
                table: "FreelancerDetails",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "FreelancerDetails",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "FreelancerDetails");

            migrationBuilder.DropColumn(
                name: "IsNotAvailableForNextSixMonth",
                table: "FreelancerDetails");

            migrationBuilder.DropColumn(
                name: "IsWeekendExclude",
                table: "FreelancerDetails");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "FreelancerDetails");
        }
    }
}
