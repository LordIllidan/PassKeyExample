using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasskeyAuth.Api.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "auth");

        migrationBuilder.CreateTable(
            name: "users",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                user_name = table.Column<string>(type: "text", nullable: true),
                name = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            schema: "auth",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateTable(
            name: "passkey_credentials",
            schema: "auth",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                credential_id = table.Column<string>(type: "text", nullable: false),
                public_key = table.Column<string>(type: "text", nullable: false),
                counter = table.Column<uint>(type: "integer", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                device_type = table.Column<string>(type: "text", nullable: false),
                user_agent = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_passkey_credentials", x => x.id);
                table.ForeignKey(
                    name: "fk_passkey_credentials_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "auth",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_passkey_credentials_credential_id",
            schema: "auth",
            table: "passkey_credentials",
            column: "credential_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_passkey_credentials_user_id",
            schema: "auth",
            table: "passkey_credentials",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "passkey_credentials",
            schema: "auth");

        migrationBuilder.DropTable(
            name: "users",
            schema: "auth");
    }
}


