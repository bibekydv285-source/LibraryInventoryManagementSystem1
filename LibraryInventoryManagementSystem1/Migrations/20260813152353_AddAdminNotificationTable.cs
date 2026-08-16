using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryInventoryManagementSystem1.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RelatedStudentId = table.Column<int>(type: "int", nullable: true),
                    RelatedBookId = table.Column<int>(type: "int", nullable: true),
                    RelatedBookIssueId = table.Column<int>(type: "int", nullable: true),
                    RelatedFineId = table.Column<int>(type: "int", nullable: true),
                    RelatedReservationId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    TriggeredByUser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TriggeredByRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminNotifications_BookIssues_RelatedBookIssueId",
                        column: x => x.RelatedBookIssueId,
                        principalTable: "BookIssues",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotifications_Books_RelatedBookId",
                        column: x => x.RelatedBookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotifications_Fines_RelatedFineId",
                        column: x => x.RelatedFineId,
                        principalTable: "Fines",
                        principalColumn: "FineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotifications_Reservations_RelatedReservationId",
                        column: x => x.RelatedReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotifications_Students_RelatedStudentId",
                        column: x => x.RelatedStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_RelatedBookId",
                table: "AdminNotifications",
                column: "RelatedBookId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_RelatedBookIssueId",
                table: "AdminNotifications",
                column: "RelatedBookIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_RelatedFineId",
                table: "AdminNotifications",
                column: "RelatedFineId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_RelatedReservationId",
                table: "AdminNotifications",
                column: "RelatedReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifications_RelatedStudentId",
                table: "AdminNotifications",
                column: "RelatedStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotifications");
        }
    }
}
