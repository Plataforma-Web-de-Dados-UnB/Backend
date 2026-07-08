using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Views
{
    public class UsuarioRegisterDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(255, ErrorMessage = "O campo Nome deve ter no máximo 255 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Sobrenome é obrigatório.")]
        [StringLength(255, ErrorMessage = "O campo Sobrenome deve ter no máximo 255 caracteres.")]
        public string UltimoNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class UsuarioLoginDto
    {
        [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
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
        [Required(ErrorMessage = "O campo Senha Atual é obrigatório.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        public string SenhaAntiga { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Nova Senha é obrigatório.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        public string SenhaNova { get; set; } = string.Empty;
    }

    public class UsuarioDeleteSelfDto
    {
        [Required(ErrorMessage = "O campo Senha é obrigatório para confirmar a exclusão.")]
        public string Senha { get; set; } = string.Empty;
    }
}
