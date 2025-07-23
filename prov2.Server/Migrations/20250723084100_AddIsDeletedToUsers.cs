using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace prov2.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProjects_Users_UserId",
                table: "UserProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProjects_Users_UserId",
                table: "UserProjects",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProjects_Users_UserId",
                table: "UserProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProjects_Users_UserId",
                table: "UserProjects",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
