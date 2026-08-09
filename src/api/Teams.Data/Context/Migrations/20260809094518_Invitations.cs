using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Teams.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class Invitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    game_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    user_id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    error = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    cursor = table.Column<long>(type: "INTEGER", nullable: false),
                    date_created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    date_deleted = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_invitations_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_cursor",
                table: "invitations",
                column: "cursor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invitations_date_created",
                table: "invitations",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_date_deleted",
                table: "invitations",
                column: "date_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_date_modified",
                table: "invitations",
                column: "date_modified");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_game_id",
                table: "invitations",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_user_id",
                table: "invitations",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations");
        }
    }
}
