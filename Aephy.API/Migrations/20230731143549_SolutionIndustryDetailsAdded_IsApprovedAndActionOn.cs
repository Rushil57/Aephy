using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class SolutionIndustryDetailsAdded_IsApprovedAndActionOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActionOn",
                table: "SolutionIndustryDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsApproved",
                table: "SolutionIndustryDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionOn",
                table: "SolutionIndustryDetails");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "SolutionIndustryDetails");
        }
    }
}
