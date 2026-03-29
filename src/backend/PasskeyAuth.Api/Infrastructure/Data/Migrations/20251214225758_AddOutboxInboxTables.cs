using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasskeyAuth.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxInboxTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "auth",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "auth",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.MessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_EventId",
                schema: "auth",
                table: "inbox_messages",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_Status_CreatedAt",
                schema: "auth",
                table: "inbox_messages",
                columns: new[] { "Status", "CreatedAt" },
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_RetryCount_CreatedAt",
                schema: "auth",
                table: "outbox_messages",
                columns: new[] { "RetryCount", "CreatedAt" },
                filter: "\"Status\" = 1 AND \"RetryCount\" < 3");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_Status_CreatedAt",
                schema: "auth",
                table: "outbox_messages",
                columns: new[] { "Status", "CreatedAt" },
                filter: "\"Status\" IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "auth");
        }
    }
}
