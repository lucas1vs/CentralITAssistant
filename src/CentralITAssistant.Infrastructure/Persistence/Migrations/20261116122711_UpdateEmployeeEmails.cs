using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralITAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_CorporateEmail",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "PersonalEmail",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorporateEmail",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CorporateEmail",
                table: "Employees",
                column: "CorporateEmail",
                unique: true,
                filter: "[CorporateEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PersonalEmail",
                table: "Employees",
                column: "PersonalEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_CorporateEmail",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PersonalEmail",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "PersonalEmail",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CorporateEmail",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CorporateEmail",
                table: "Employees",
                column: "CorporateEmail",
                unique: true);
        }
    }
}
