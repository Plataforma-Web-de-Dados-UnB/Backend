using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddPipelineBronze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bronze");

            migrationBuilder.EnsureSchema(
                name: "pipeline");

            migrationBuilder.CreateTable(
                name: "arquivos_brutos",
                schema: "bronze",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_linha = table.Column<int>(type: "integer", nullable: false),
                    dados = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arquivos_brutos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pipelines",
                schema: "pipeline",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    script_python = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pipelines", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "execucoes",
                schema: "pipeline",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pipeline_id = table.Column<int>(type: "integer", nullable: false),
                    tabela_silver = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tabela_gold = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: true),
                    iniciado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finalizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_execucoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_execucoes_pipelines_pipeline_id",
                        column: x => x.pipeline_id,
                        principalSchema: "pipeline",
                        principalTable: "pipelines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_arquivos_brutos_batch_id",
                schema: "bronze",
                table: "arquivos_brutos",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_execucoes_batch_id",
                schema: "pipeline",
                table: "execucoes",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_execucoes_pipeline_id",
                schema: "pipeline",
                table: "execucoes",
                column: "pipeline_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arquivos_brutos",
                schema: "bronze");

            migrationBuilder.DropTable(
                name: "execucoes",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "pipelines",
                schema: "pipeline");
        }
    }
}
