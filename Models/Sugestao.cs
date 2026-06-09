using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public enum TipoSugestao
{
    Sugestao,
    Erro,
    Relato
}

public enum StatusSugestao
{
    Pendente,
    Analisado,
    Descartado
}

[Table("SUGESTAO")]
public class Sugestao
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tipo")]
    [EnumDataType(typeof(TipoSugestao))]
    public TipoSugestao Tipo { get; set; }

    [Required]
    [StringLength(255)]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(255)]
    [Column("nome_contato")]
    public string? NomeContato { get; set; }

    [StringLength(255)]
    [Column("email_contato")]
    public string? EmailContato { get; set; }

    [Required]
    [Column("status")]
    [EnumDataType(typeof(StatusSugestao))]
    public StatusSugestao Status { get; set; } = StatusSugestao.Pendente;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
