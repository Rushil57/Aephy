using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Field_table_EphylinkRevoultAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevolutStatus",
                table: "EphylinkRevolutAccount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RevolutStatus",
                table: "EphylinkRevolutAccount",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
