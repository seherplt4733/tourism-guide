using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismGuide.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureAndReviewImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewImageUrl",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewImageUrl",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");
        }
    }
}
