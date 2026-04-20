using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KanbanApp.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeMovementHistoryColumnsFKNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMovementHistories_Columns_FromColumnId",
                table: "TicketMovementHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMovementHistories_Columns_ToColumnId",
                table: "TicketMovementHistories");

            migrationBuilder.AlterColumn<int>(
                name: "ToColumnId",
                table: "TicketMovementHistories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "FromColumnId",
                table: "TicketMovementHistories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMovementHistories_Columns_FromColumnId",
                table: "TicketMovementHistories",
                column: "FromColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMovementHistories_Columns_ToColumnId",
                table: "TicketMovementHistories",
                column: "ToColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMovementHistories_Columns_FromColumnId",
                table: "TicketMovementHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMovementHistories_Columns_ToColumnId",
                table: "TicketMovementHistories");

            migrationBuilder.AlterColumn<int>(
                name: "ToColumnId",
                table: "TicketMovementHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FromColumnId",
                table: "TicketMovementHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMovementHistories_Columns_FromColumnId",
                table: "TicketMovementHistories",
                column: "FromColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMovementHistories_Columns_ToColumnId",
                table: "TicketMovementHistories",
                column: "ToColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
