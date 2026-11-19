using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralITAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingRegistrationResendFields : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCodeSentAt",
                table: "PendingEmployeeRegistrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResendCount",
                table: "PendingEmployeeRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
        UPDATE PendingEmployeeRegistrations
        SET LastCodeSentAt = CreatedAt;
        """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastCodeSentAt",
                table: "PendingEmployeeRegistrations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCodeSentAt",
                table: "PendingEmployeeRegistrations");

            migrationBuilder.DropColumn(
                name: "ResendCount",
                table: "PendingEmployeeRegistrations");
        }
    }
}
