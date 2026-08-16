using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryInventoryManagementSystem1.Migrations
{
    /// <inheritdoc />
    public partial class FixFineIssueIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BookIssues_BookIssueIssueId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_BookIssueIssueId",
                table: "Fines");

            migrationBuilder.DropColumn(
                name: "BookIssueIssueId",
                table: "Fines");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_IssueId",
                table: "Fines",
                column: "IssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BookIssues_IssueId",
                table: "Fines",
                column: "IssueId",
                principalTable: "BookIssues",
                principalColumn: "IssueId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BookIssues_IssueId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_IssueId",
                table: "Fines");

            migrationBuilder.AddColumn<int>(
                name: "BookIssueIssueId",
                table: "Fines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fines_BookIssueIssueId",
                table: "Fines",
                column: "BookIssueIssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BookIssues_BookIssueIssueId",
                table: "Fines",
                column: "BookIssueIssueId",
                principalTable: "BookIssues",
                principalColumn: "IssueId");
        }
    }
}
