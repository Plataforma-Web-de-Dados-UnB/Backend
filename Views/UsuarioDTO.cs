using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Views
{
    public class UsuarioRegisterDto
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string UltimoNome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Senha { get; set; } = string.Empty;
    }

    public class UsuarioLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Senha { get; set; } = string.Empty;
    }

    public class UsuarioLoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string UltimoNome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CargoUsuario Cargo { get; set; }
    }

    public class AuthRefreshResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class UsuarioGetDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string UltimoNome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CargoUsuario Cargo { get; set; }
        public StatusUsuario Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UsuarioListDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string UltimoNome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CargoUsuario Cargo { get; set; }
        public StatusUsuario Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UsuarioUpdateStatusDto
    {
        [Required]
        [EnumDataType(typeof(StatusUsuario))]
        public StatusUsuario Status { get; set; }
    }

    public class UsuarioChangePasswordDto
    {
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string SenhaAntiga { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string SenhaNova { get; set; } = string.Empty;
    }
}
