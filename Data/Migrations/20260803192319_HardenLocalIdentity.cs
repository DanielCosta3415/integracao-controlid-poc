using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Integracao.ControlID.PoC.Data.Migrations
{
    /// <inheritdoc />
    public partial class HardenLocalIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "TEXT",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUsername",
                table: "Users",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE Users
                SET NormalizedUsername = UPPER(TRIM(Username)),
                    NormalizedEmail = UPPER(TRIM(Email));
                """);

            migrationBuilder.CreateIndex(
                name: "UX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Users_NormalizedUsername",
                table: "Users",
                column: "NormalizedUsername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UX_Users_NormalizedUsername",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedUsername",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username");
        }
    }
}
