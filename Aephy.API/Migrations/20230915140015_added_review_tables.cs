using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class added_review_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FreelancerReview",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FreelancerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Feedback_Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommunicationRating = table.Column<int>(type: "int", nullable: true),
                    CollaborationRating = table.Column<int>(type: "int", nullable: true),
                    ProfessionalismRating = table.Column<int>(type: "int", nullable: true),
                    TechnicalRating = table.Column<int>(type: "int", nullable: true),
                    SatisfactionRating = table.Column<int>(type: "int", nullable: true),
                    ResponsivenessRating = table.Column<int>(type: "int", nullable: true),
                    LikeToWorkRating = table.Column<int>(type: "int", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelancerReview", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProjectReview",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolutionId = table.Column<int>(type: "int", nullable: false),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    Feedback_Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WellDefinedProjectScope = table.Column<int>(type: "int", nullable: true),
                    AdherenceToProjectScope = table.Column<int>(type: "int", nullable: true),
                    DeliverablesQuality = table.Column<int>(type: "int", nullable: true),
                    MeetingTimeliness = table.Column<int>(type: "int", nullable: true),
                    Clientsatisfaction = table.Column<int>(type: "int", nullable: true),
                    AdherenceToBudget = table.Column<int>(type: "int", nullable: true),
                    LikeToRecommend = table.Column<int>(type: "int", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectReview", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreelancerReview");

            migrationBuilder.DropTable(
                name: "ProjectReview");
        }
    }
}
