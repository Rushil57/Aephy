using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class AdminToFreelancerReview_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminToFreelancerReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreelancerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feedback_Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Professionalism = table.Column<int>(type: "int", nullable: true),
                    HourlyRate = table.Column<int>(type: "int", nullable: true),
                    Availability = table.Column<int>(type: "int", nullable: true),
                    ProjectAcceptance = table.Column<int>(type: "int", nullable: true),
                    Education = table.Column<int>(type: "int", nullable: true),
                    SoftSkillsExperience = table.Column<int>(type: "int", nullable: true),
                    HardSkillsExperience = table.Column<int>(type: "int", nullable: true),
                    ProjectSuccessRate = table.Column<int>(type: "int", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminToFreelancerReview", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminToFreelancerReview");
        }
    }
}
