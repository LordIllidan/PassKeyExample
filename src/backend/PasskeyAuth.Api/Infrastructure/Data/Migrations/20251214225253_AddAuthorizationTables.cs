using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasskeyAuth.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorizationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "authorization_challenges",
                schema: "auth",
                columns: table => new
                {
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OperationData = table.Column<string>(type: "jsonb", nullable: false),
                    ChallengeCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MethodType = table.Column<int>(type: "integer", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authorization_challenges", x => x.ChallengeId);
                    table.ForeignKey(
                        name: "FK_authorization_challenges_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "authorization_tokens",
                schema: "auth",
                columns: table => new
                {
                    TokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authorization_tokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_authorization_tokens_authorization_challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalSchema: "auth",
                        principalTable: "authorization_challenges",
                        principalColumn: "ChallengeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_authorization_tokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_authorization_challenges_ExpiresAt",
                schema: "auth",
                table: "authorization_challenges",
                column: "ExpiresAt",
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_challenges_Status_ExpiresAt",
                schema: "auth",
                table: "authorization_challenges",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_authorization_challenges_UserId",
                schema: "auth",
                table: "authorization_challenges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_tokens_ChallengeId",
                schema: "auth",
                table: "authorization_tokens",
                column: "ChallengeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_authorization_tokens_ExpiresAt",
                schema: "auth",
                table: "authorization_tokens",
                column: "ExpiresAt",
                filter: "\"IsUsed\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_tokens_Token",
                schema: "auth",
                table: "authorization_tokens",
                column: "Token",
                unique: true,
                filter: "\"IsUsed\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_authorization_tokens_UserId",
                schema: "auth",
                table: "authorization_tokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authorization_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "authorization_challenges",
                schema: "auth");
        }
    }
}
