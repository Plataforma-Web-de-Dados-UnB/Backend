using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEmbedDashboardUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "embed_dashboard_uuid",
                table: "PAINEL",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "embed_dashboard_uuid",
                table: "PAINEL");
        }
    }
}
