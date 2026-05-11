using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraReportingSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurposeAndCreatedAtToOtpVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OtpVerifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "OtpVerifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "OtpVerifications");
        }
    }
}
