using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REFRESH_TOKEN",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    family_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFRESH_TOKEN", x => x.id);
                    table.ForeignKey(
                        name: "FK_REFRESH_TOKEN_AspNetUsers_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKEN_token",
                table: "REFRESH_TOKEN",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKEN_usuario_id",
                table: "REFRESH_TOKEN",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REFRESH_TOKEN");
        }
    }
}
