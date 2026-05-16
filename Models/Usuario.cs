using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public enum CargoUsuario
{
    SuperAdministrador,
    Administrador,
    Visitante
}

public enum StatusUsuario
{
    Pendente,
    Ativo,
    Recusado
}

[Table("USUARIO")]
public class Usuario : IdentityUser
{
    [Required]
    [StringLength(255)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("ultimo_nome")]
    public string UltimoNome { get; set; } = string.Empty;

    [Required]
    [Column("cargo")]
    [EnumDataType(typeof(CargoUsuario))]
    public CargoUsuario Cargo { get; set; }

    [Required]
    [Column("status")]
    [EnumDataType(typeof(StatusUsuario))]
    public StatusUsuario Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
