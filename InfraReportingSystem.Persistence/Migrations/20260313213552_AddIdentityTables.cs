using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraReportingSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Roles_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_OtpVerifications_AspNetUsers_UserId1",
                table: "OtpVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_AuthorityId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_WorkerId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AuthorityId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_OtpVerifications_UserId1",
                table: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AuthorityId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "Pic_url",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "Reports",
                newName: "AssignedWorkerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reports",
                newName: "AssignedByAuthorityId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_WorkerId",
                table: "Reports",
                newName: "IX_Reports_AssignedWorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                newName: "IX_Reports_AssignedByAuthorityId");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedById",
                table: "Reports",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "OtpVerifications",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SubmittedById",
                table: "Reports",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_UserId",
                table: "OtpVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OtpVerifications_AspNetUsers_UserId",
                table: "OtpVerifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_AssignedByAuthorityId",
                table: "Reports",
                column: "AssignedByAuthorityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_AssignedWorkerId",
                table: "Reports",
                column: "AssignedWorkerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_SubmittedById",
                table: "Reports",
                column: "SubmittedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtpVerifications_AspNetUsers_UserId",
                table: "OtpVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_AssignedByAuthorityId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_AssignedWorkerId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_SubmittedById",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_Reports_SubmittedById",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_OtpVerifications_UserId",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SubmittedById",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "AssignedWorkerId",
                table: "Reports",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "AssignedByAuthorityId",
                table: "Reports",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_AssignedWorkerId",
                table: "Reports",
                newName: "IX_Reports_WorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_AssignedByAuthorityId",
                table: "Reports",
                newName: "IX_Reports_UserId");

            migrationBuilder.AddColumn<string>(
                name: "AuthorityId",
                table: "Reports",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "OtpVerifications",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "OtpVerifications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Pic_url",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AuthorityId",
                table: "Reports",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_UserId1",
                table: "OtpVerifications",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Roles_RoleId",
                table: "AspNetUsers",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtpVerifications_AspNetUsers_UserId1",
                table: "OtpVerifications",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_AuthorityId",
                table: "Reports",
                column: "AuthorityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_WorkerId",
                table: "Reports",
                column: "WorkerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
