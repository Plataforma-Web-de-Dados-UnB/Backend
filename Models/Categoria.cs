using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("CATEGORIA")]
public class Categoria
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

    [StringLength(512)]
    [Column("imagem_path")]
    public string? ImagemPath { get; set; }

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

    public ICollection<Painel> Paineis { get; set; } = [];
}
