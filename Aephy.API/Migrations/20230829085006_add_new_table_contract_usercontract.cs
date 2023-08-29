using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class add_new_table_contract_usercontract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contract",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MileStoneId = table.Column<int>(type: "int", nullable: false),
                    SolutionId = table.Column<int>(type: "int", nullable: false),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    ClientUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionStatus = table.Column<int>(type: "int", nullable: false),
                    SessionExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LatestChargeId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contract_AspNetUsers_ClientUserId",
                        column: x => x.ClientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contract_SolutionMilestone_MileStoneId",
                        column: x => x.MileStoneId,
                        principalTable: "SolutionMilestone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Percentage = table.Column<int>(type: "int", nullable: false),
                    StripeTranferId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTransfered = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractUser_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractUser_Contract_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contract_ClientUserId",
                table: "Contract",
                column: "ClientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_MileStoneId",
                table: "Contract",
                column: "MileStoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractUser_ApplicationUserId",
                table: "ContractUser",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractUser_ContractId",
                table: "ContractUser",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractUser");

            migrationBuilder.DropTable(
                name: "Contract");
        }
    }
}
