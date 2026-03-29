using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasskeyAuth.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Counter",
                schema: "auth",
                table: "passkey_credentials",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "two_factor_auths",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SecretKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BackupCodes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_two_factor_auths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_two_factor_auths_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_two_factor_auths_UserId",
                schema: "auth",
                table: "two_factor_auths",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_two_factor_auths_enabled",
                schema: "auth",
                table: "two_factor_auths",
                column: "IsEnabled",
                filter: "\"IsEnabled\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "two_factor_auths",
                schema: "auth");

            migrationBuilder.AlterColumn<int>(
                name: "Counter",
                schema: "auth",
                table: "passkey_credentials",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
