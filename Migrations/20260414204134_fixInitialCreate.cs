using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KanbanApp.API.Migrations
{
    /// <inheritdoc />
    public partial class fixInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KanbanMember_Kanbans_KanbanId",
                table: "KanbanMember");

            migrationBuilder.DropForeignKey(
                name: "FK_KanbanMember_Users_UserId",
                table: "KanbanMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KanbanMember",
                table: "KanbanMember");

            migrationBuilder.RenameTable(
                name: "KanbanMember",
                newName: "KanbanMembers");

            migrationBuilder.RenameIndex(
                name: "IX_KanbanMember_UserId",
                table: "KanbanMembers",
                newName: "IX_KanbanMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_KanbanMember_KanbanId",
                table: "KanbanMembers",
                newName: "IX_KanbanMembers_KanbanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KanbanMembers",
                table: "KanbanMembers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KanbanMembers_Kanbans_KanbanId",
                table: "KanbanMembers",
                column: "KanbanId",
                principalTable: "Kanbans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KanbanMembers_Users_UserId",
                table: "KanbanMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KanbanMembers_Kanbans_KanbanId",
                table: "KanbanMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_KanbanMembers_Users_UserId",
                table: "KanbanMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KanbanMembers",
                table: "KanbanMembers");

            migrationBuilder.RenameTable(
                name: "KanbanMembers",
                newName: "KanbanMember");

            migrationBuilder.RenameIndex(
                name: "IX_KanbanMembers_UserId",
                table: "KanbanMember",
                newName: "IX_KanbanMember_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_KanbanMembers_KanbanId",
                table: "KanbanMember",
                newName: "IX_KanbanMember_KanbanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KanbanMember",
                table: "KanbanMember",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KanbanMember_Kanbans_KanbanId",
                table: "KanbanMember",
                column: "KanbanId",
                principalTable: "Kanbans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KanbanMember_Users_UserId",
                table: "KanbanMember",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
