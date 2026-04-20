using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KanbanApp.API.Migrations
{
    /// <inheritdoc />
    public partial class NewInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kanbans_Users_UserId",
                table: "Kanbans");

            migrationBuilder.DropIndex(
                name: "IX_Kanbans_UserId",
                table: "Kanbans");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Kanbans",
                newName: "CreatedByUserId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Kanbans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "KanbanMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Role = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    KanbanId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanbanMember_Kanbans_KanbanId",
                        column: x => x.KanbanId,
                        principalTable: "Kanbans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KanbanMember_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kanbans_CreatedByUserId",
                table: "Kanbans",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanMember_KanbanId",
                table: "KanbanMember",
                column: "KanbanId");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanMember_UserId",
                table: "KanbanMember",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kanbans_Users_CreatedByUserId",
                table: "Kanbans",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kanbans_Users_CreatedByUserId",
                table: "Kanbans");

            migrationBuilder.DropTable(
                name: "KanbanMember");

            migrationBuilder.DropIndex(
                name: "IX_Kanbans_CreatedByUserId",
                table: "Kanbans");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Kanbans");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Kanbans",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Kanbans_UserId",
                table: "Kanbans",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Kanbans_Users_UserId",
                table: "Kanbans",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
