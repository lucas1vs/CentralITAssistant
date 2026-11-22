using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralITAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingWhatsAppVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingWhatsAppVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastCodeSentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResendCount = table.Column<int>(type: "int", nullable: false),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingWhatsAppVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingWhatsAppVerifications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingWhatsAppVerifications_EmployeeId",
                table: "PendingWhatsAppVerifications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingWhatsAppVerifications_PhoneNumber",
                table: "PendingWhatsAppVerifications",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingWhatsAppVerifications");
        }
    }
}
