using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class addedSolFundIdTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractUser_Contract_ContractId",
                table: "ContractUser");

            migrationBuilder.AlterColumn<int>(
                name: "ContractId",
                table: "ContractUser",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SolutionFundId",
                table: "Contract",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractUser_Contract_ContractId",
                table: "ContractUser",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractUser_Contract_ContractId",
                table: "ContractUser");

            migrationBuilder.DropColumn(
                name: "SolutionFundId",
                table: "Contract");

            migrationBuilder.AlterColumn<int>(
                name: "ContractId",
                table: "ContractUser",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractUser_Contract_ContractId",
                table: "ContractUser",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id");
        }
    }
}
