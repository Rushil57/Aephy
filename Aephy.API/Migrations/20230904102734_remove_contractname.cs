using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class remove_contractname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contract_SolutionMilestone_MileStoneId",
                table: "Contract");

            migrationBuilder.DropIndex(
                name: "IX_Contract_MileStoneId",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "MileStoneId",
                table: "Contract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MileStoneId",
                table: "Contract",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Contract_MileStoneId",
                table: "Contract",
                column: "MileStoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_SolutionMilestone_MileStoneId",
                table: "Contract",
                column: "MileStoneId",
                principalTable: "SolutionMilestone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
