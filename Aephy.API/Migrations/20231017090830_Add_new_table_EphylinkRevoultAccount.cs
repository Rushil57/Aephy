using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_new_table_EphylinkRevoultAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EphylinkRevolutAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevolutConnectId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevolutAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevolutStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EphylinkRevolutAccount", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EphylinkRevolutAccount");
        }
    }
}
