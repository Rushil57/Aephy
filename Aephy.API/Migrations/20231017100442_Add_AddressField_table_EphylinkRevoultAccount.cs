using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_AddressField_table_EphylinkRevoultAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "EphylinkRevolutAccount",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "EphylinkRevolutAccount",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "EphylinkRevolutAccount",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostCode",
                table: "EphylinkRevolutAccount",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "EphylinkRevolutAccount");

            migrationBuilder.DropColumn(
                name: "City",
                table: "EphylinkRevolutAccount");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "EphylinkRevolutAccount");

            migrationBuilder.DropColumn(
                name: "PostCode",
                table: "EphylinkRevolutAccount");
        }
    }
}
