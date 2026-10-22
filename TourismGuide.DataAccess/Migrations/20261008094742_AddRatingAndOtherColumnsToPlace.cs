using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismGuide.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingAndOtherColumnsToPlace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Reviews",
                newName: "Rating");

            migrationBuilder.AddColumn<int>(
                name: "PlaceId1",
                table: "ItineraryPlaces",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryPlaces_PlaceId1",
                table: "ItineraryPlaces",
                column: "PlaceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ItineraryPlaces_Places_PlaceId1",
                table: "ItineraryPlaces",
                column: "PlaceId1",
                principalTable: "Places",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItineraryPlaces_Places_PlaceId1",
                table: "ItineraryPlaces");

            migrationBuilder.DropIndex(
                name: "IX_ItineraryPlaces_PlaceId1",
                table: "ItineraryPlaces");

            migrationBuilder.DropColumn(
                name: "PlaceId1",
                table: "ItineraryPlaces");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Reviews",
                newName: "Score");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
