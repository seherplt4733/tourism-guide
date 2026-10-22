using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourismGuide.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[Reviews]') AND name = 'IsApproved'
                )
                BEGIN
                    ALTER TABLE [Reviews] ADD [IsApproved] bit NOT NULL CONSTRAINT [DF_Reviews_IsApproved] DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.tables 
                    WHERE name = 'ContactMessages'
                )
                BEGIN
                    CREATE TABLE [ContactMessages] (
                        [Id] int NOT NULL IDENTITY,
                        [FullName] nvarchar(max) NOT NULL,
                        [Email] nvarchar(max) NOT NULL,
                        [Subject] nvarchar(max) NOT NULL,
                        [Message] nvarchar(max) NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [IsRead] bit NOT NULL,
                        CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
                    );
                END
            ");

            migrationBuilder.CreateTable(
                name: "EventAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventAttendances_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventAttendances_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_EventId",
                table: "EventAttendances",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_UserId",
                table: "EventAttendances",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "EventAttendances");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Reviews");
        }
    }
}
