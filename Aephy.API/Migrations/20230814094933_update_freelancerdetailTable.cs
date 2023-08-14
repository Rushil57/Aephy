using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aephy.API.Migrations
{
    /// <inheritdoc />
    public partial class update_freelancerdetailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageBlobStorageBaseUrl",
                table: "FreelancerDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "FreelancerDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlWithSas",
                table: "FreelancerDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBlobStorageBaseUrl",
                table: "FreelancerDetails");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FreelancerDetails");

            migrationBuilder.DropColumn(
                name: "ImageUrlWithSas",
                table: "FreelancerDetails");
        }
    }
}
