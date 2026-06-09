using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("REFRESH_TOKEN")]
public class RefreshToken
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(128)]
    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(36)]
    [Column("family_id")]
    public string FamilyId { get; set; } = string.Empty;

    [Required]
    [Column("usuario_id")]
    public string UsuarioId { get; set; } = string.Empty;

    [ForeignKey("UsuarioId")]
    public Usuario Usuario { get; set; } = null!;

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsRevoked => RevokedAt.HasValue;

    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;
}
