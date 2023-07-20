using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class OpenGigRolesApplications2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrlWithSas",
                table: "OpenGigRolesApplications",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "OpenGigRolesApplications",
                newName: "CVUrlWithSas");

            migrationBuilder.AddColumn<string>(
                name: "CVPath",
                table: "OpenGigRolesApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVPath",
                table: "OpenGigRolesApplications");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "OpenGigRolesApplications",
                newName: "ImageUrlWithSas");

            migrationBuilder.RenameColumn(
                name: "CVUrlWithSas",
                table: "OpenGigRolesApplications",
                newName: "ImagePath");
        }
    }
}
