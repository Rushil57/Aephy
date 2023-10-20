using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_TransferAmount_SolutionTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TransferAmount",
                table: "SolutionTeam",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferAmount",
                table: "SolutionTeam");
        }
    }
}
