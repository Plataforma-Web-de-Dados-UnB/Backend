using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "uploads_auditoria",
                schema: "bronze",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_hash = table.Column<string>(type: "text", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "text", nullable: false),
                    total_linhas = table.Column<int>(type: "integer", nullable: false),
                    uploaded_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uploads_auditoria", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_uploads_auditoria_file_hash",
                schema: "bronze",
                table: "uploads_auditoria",
                column: "file_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "uploads_auditoria",
                schema: "bronze");
        }
    }
}
