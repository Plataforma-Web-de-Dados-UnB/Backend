using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("PAINEL")]
public class Painel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Required]
    [StringLength(2048)]
    [Column("graph_embed_link")]
    public string GraphEmbedLink { get; set; } = string.Empty;

    [StringLength(255)]
    [Column("embed_dashboard_uuid")]
    public string? EmbedDashboardUuid { get; set; }

    [Column("sort_ordem")]
    public int SortOrdem { get; set; } = 0;

    [Required]
    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("deactivated_at")]
    public DateTime? DeactivatedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("categoria_id")]
    public int CategoriaId { get; set; }

    [ForeignKey("CategoriaId")]
    public Categoria Categoria { get; set; } = null!;
}
