using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class OpenGigRolesApplications1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobStorageBaseUrl",
                table: "OpenGigRolesApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "OpenGigRolesApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlWithSas",
                table: "OpenGigRolesApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobStorageBaseUrl",
                table: "OpenGigRolesApplications");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "OpenGigRolesApplications");

            migrationBuilder.DropColumn(
                name: "ImageUrlWithSas",
                table: "OpenGigRolesApplications");
        }
    }
}
