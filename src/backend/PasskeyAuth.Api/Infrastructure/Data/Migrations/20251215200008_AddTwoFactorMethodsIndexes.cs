using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasskeyAuth.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorMethodsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_two_factor_methods_enabled",
                schema: "auth",
                table: "two_factor_methods",
                columns: new[] { "UserId", "IsEnabled" },
                filter: "\"IsEnabled\" = true");

            migrationBuilder.CreateIndex(
                name: "idx_two_factor_methods_user_primary",
                schema: "auth",
                table: "two_factor_methods",
                columns: new[] { "UserId", "IsPrimary" },
                filter: "\"IsPrimary\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_two_factor_methods_enabled",
                schema: "auth",
                table: "two_factor_methods");

            migrationBuilder.DropIndex(
                name: "idx_two_factor_methods_user_primary",
                schema: "auth",
                table: "two_factor_methods");
        }
    }
}
