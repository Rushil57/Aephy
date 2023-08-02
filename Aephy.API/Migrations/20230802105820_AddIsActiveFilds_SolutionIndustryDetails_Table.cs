﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveFilds_SolutionIndustryDetails_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActiveForClient",
                table: "SolutionIndustryDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveForFreelancer",
                table: "SolutionIndustryDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActiveForClient",
                table: "SolutionIndustryDetails");

            migrationBuilder.DropColumn(
                name: "IsActiveForFreelancer",
                table: "SolutionIndustryDetails");
        }
    }
}
