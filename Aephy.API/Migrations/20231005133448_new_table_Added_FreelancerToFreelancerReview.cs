using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class new_table_Added_FreelancerToFreelancerReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FreelancerToFreelancerReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToFreelancerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromFreelancerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolutionId = table.Column<int>(type: "int", nullable: false),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    Feedback_Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollaborationAndTeamWork = table.Column<int>(type: "int", nullable: false),
                    Communication = table.Column<int>(type: "int", nullable: false),
                    Professionalism = table.Column<int>(type: "int", nullable: false),
                    TechnicalSkills = table.Column<int>(type: "int", nullable: false),
                    ProjectManagement = table.Column<int>(type: "int", nullable: false),
                    Responsiveness = table.Column<int>(type: "int", nullable: false),
                    WellDefinedProjectScope = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelancerToFreelancerReview", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreelancerToFreelancerReview");
        }
    }
}
