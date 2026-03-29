using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasskeyAuth.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSecurityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "auth",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "jsonb", nullable: false),
                    IpAddress = table.Column<IPAddress>(type: "inet", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "rate_limit_entries",
                schema: "auth",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpAddress = table.Column<IPAddress>(type: "inet", nullable: true),
                    OperationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    WindowStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindowEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate_limit_entries", x => x.EntryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAt",
                schema: "auth",
                table: "audit_logs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EventCategory_CreatedAt",
                schema: "auth",
                table: "audit_logs",
                columns: new[] { "EventCategory", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EventType_CreatedAt",
                schema: "auth",
                table: "audit_logs",
                columns: new[] { "EventType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId_CreatedAt",
                schema: "auth",
                table: "audit_logs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_rate_limit_entries_BlockedUntil",
                schema: "auth",
                table: "rate_limit_entries",
                column: "BlockedUntil",
                filter: "\"IsBlocked\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_rate_limit_entries_IpAddress_OperationType_WindowStart",
                schema: "auth",
                table: "rate_limit_entries",
                columns: new[] { "IpAddress", "OperationType", "WindowStart" });

            migrationBuilder.CreateIndex(
                name: "IX_rate_limit_entries_UserId_OperationType_WindowStart",
                schema: "auth",
                table: "rate_limit_entries",
                columns: new[] { "UserId", "OperationType", "WindowStart" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "rate_limit_entries",
                schema: "auth");
        }
    }
}
