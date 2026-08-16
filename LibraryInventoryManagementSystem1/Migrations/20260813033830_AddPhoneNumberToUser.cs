using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryInventoryManagementSystem1.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BookIssues_BookIssueIssueId",
                table: "Fines");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BookIssueIssueId",
                table: "Fines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BookIssues_BookIssueIssueId",
                table: "Fines",
                column: "BookIssueIssueId",
                principalTable: "BookIssues",
                principalColumn: "IssueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BookIssues_BookIssueIssueId",
                table: "Fines");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "BookIssueIssueId",
                table: "Fines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BookIssues_BookIssueIssueId",
                table: "Fines",
                column: "BookIssueIssueId",
                principalTable: "BookIssues",
                principalColumn: "IssueId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
