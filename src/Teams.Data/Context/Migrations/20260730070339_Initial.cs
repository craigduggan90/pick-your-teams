using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Teams.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    start_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    home_rating = table.Column<int>(type: "INTEGER", nullable: true),
                    away_rating = table.Column<int>(type: "INTEGER", nullable: true),
                    winner = table.Column<int>(type: "INTEGER", nullable: true),
                    team_size = table.Column<int>(type: "INTEGER", nullable: false),
                    cursor = table.Column<long>(type: "INTEGER", nullable: false),
                    date_created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_deleted = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    rating = table.Column<int>(type: "INTEGER", nullable: false),
                    cursor = table.Column<long>(type: "INTEGER", nullable: false),
                    date_created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_deleted = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_cursor",
                table: "game",
                column: "cursor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_date_created",
                table: "game",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "IX_game_date_deleted",
                table: "game",
                column: "date_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_game_date_modified",
                table: "game",
                column: "date_modified");

            migrationBuilder.CreateIndex(
                name: "IX_players_cursor",
                table: "players",
                column: "cursor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_date_created",
                table: "players",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "IX_players_date_deleted",
                table: "players",
                column: "date_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_players_date_modified",
                table: "players",
                column: "date_modified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game");

            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
