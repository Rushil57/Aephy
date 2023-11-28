using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_New_FreelancerFindProcess_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FreelancerFindProcessDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FreelancerFindProcessHeaderId = table.Column<int>(type: "int", nullable: false),
                    AlgorithumStage = table.Column<int>(type: "int", nullable: false),
                    FreelancerType = table.Column<int>(type: "int", nullable: false),
                    FreelancerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApproveStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelancerFindProcessDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FreelancerFindProcessHeader",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolutionId = table.Column<int>(type: "int", nullable: false),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    ProjectType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentAlgorithumStage = table.Column<int>(type: "int", nullable: false),
                    TotalProjectManager = table.Column<int>(type: "int", nullable: false),
                    TotalAssociate = table.Column<int>(type: "int", nullable: false),
                    TotalExpert = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecuteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsTeamCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelancerFindProcessHeader", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreelancerFindProcessDetails");

            migrationBuilder.DropTable(
                name: "FreelancerFindProcessHeader");
        }
    }
}
