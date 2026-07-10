using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Views
{
    public class SugestaoCreateDto
    {
        [Required(ErrorMessage = "O tipo de solicitação é obrigatório.")]
        [EnumDataType(typeof(TipoSugestao), ErrorMessage = "Tipo de solicitação inválido.")]
        public TipoSugestao Tipo { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(255, ErrorMessage = "O título deve ter no máximo 255 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome de contato é obrigatório.")]
        [StringLength(255, ErrorMessage = "O nome de contato deve ter no máximo 255 caracteres.")]
        public string NomeContato { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail de contato é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail de contato informado não é válido.")]
        [StringLength(255, ErrorMessage = "O e-mail de contato deve ter no máximo 255 caracteres.")]
        public string EmailContato { get; set; } = string.Empty;
    }

    public class SugestaoUpdateStatusDto
    {
        [Required(ErrorMessage = "O status é obrigatório.")]
        [EnumDataType(typeof(StatusSugestao), ErrorMessage = "Status inválido.")]
        public StatusSugestao Status { get; set; }
    }

    public class SugestaoGetDto
    {
        public int Id { get; set; }
        public TipoSugestao Tipo { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? NomeContato { get; set; }
        public string? EmailContato { get; set; }
        public StatusSugestao Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SugestaoListDto
    {
        public int Id { get; set; }
        public TipoSugestao Tipo { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? NomeContato { get; set; }
        public string? EmailContato { get; set; }
        public StatusSugestao Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
