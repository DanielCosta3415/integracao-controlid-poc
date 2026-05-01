using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Integracao.ControlID.PoC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "Operator");

            migrationBuilder.Sql(
                """
                UPDATE Users
                SET Role = 'Administrator'
                WHERE Id = (SELECT MIN(Id) FROM Users);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
