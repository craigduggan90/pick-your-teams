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
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    idp_id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    phone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    tag = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    rating = table.Column<int>(type: "INTEGER", nullable: false),
                    cursor = table.Column<long>(type: "INTEGER", nullable: false),
                    date_created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_deleted = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    start_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    organiser = table.Column<string>(type: "TEXT", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_game_users_organiser",
                        column: x => x.organiser,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    game_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    display_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    rating = table.Column<int>(type: "INTEGER", nullable: false),
                    delta = table.Column<int>(type: "INTEGER", nullable: true),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    team = table.Column<int>(type: "INTEGER", nullable: false),
                    cursor = table.Column<long>(type: "INTEGER", nullable: false),
                    date_created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_deleted = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                    table.ForeignKey(
                        name: "FK_players_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_players_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
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
                name: "IX_game_organiser",
                table: "game",
                column: "organiser");

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

            migrationBuilder.CreateIndex(
                name: "IX_players_game_id",
                table: "players",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_user_id",
                table: "players",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_cursor",
                table: "users",
                column: "cursor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_date_created",
                table: "users",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "IX_users_date_deleted",
                table: "users",
                column: "date_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_users_date_modified",
                table: "users",
                column: "date_modified");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_tag",
                table: "users",
                column: "tag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "game");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
