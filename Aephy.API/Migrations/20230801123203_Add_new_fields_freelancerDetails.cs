using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_new_fields_freelancerDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobStorageBaseUrl",
                table: "FreelancerDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVPath",
                table: "FreelancerDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVUrlWithSas",
                table: "FreelancerDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobStorageBaseUrl",
                table: "FreelancerDetails");

            migrationBuilder.DropColumn(
                name: "CVPath",
                table: "FreelancerDetails");

            migrationBuilder.DropColumn(
                name: "CVUrlWithSas",
                table: "FreelancerDetails");
        }
    }
}
