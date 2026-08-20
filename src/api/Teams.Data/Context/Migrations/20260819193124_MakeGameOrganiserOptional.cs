using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Teams.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class MakeGameOrganiserOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_users_organiser",
                table: "game");

            migrationBuilder.AlterColumn<string>(
                name: "organiser",
                table: "game",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_game_users_organiser",
                table: "game",
                column: "organiser",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_users_organiser",
                table: "game");

            migrationBuilder.AlterColumn<string>(
                name: "organiser",
                table: "game",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_game_users_organiser",
                table: "game",
                column: "organiser",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
