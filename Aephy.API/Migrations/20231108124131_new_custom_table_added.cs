using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class new_custom_table_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomProjectDetials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolutionDefineId = table.Column<int>(type: "int", nullable: false),
                    ProjectDuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartHour = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndHour = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvailableDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Associate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expert = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectManager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSingleFreelancer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomProjectDetials", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomProjectDetials");
        }
    }
}
