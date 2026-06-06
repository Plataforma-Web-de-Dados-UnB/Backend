using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Views
{
    public class SugestaoCreateDto
    {
        [Required]
        [EnumDataType(typeof(TipoSugestao))]
        public TipoSugestao Tipo { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(255)]
        public string? NomeContato { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? EmailContato { get; set; }
    }

    public class SugestaoUpdateStatusDto
    {
        [Required]
        [EnumDataType(typeof(StatusSugestao))]
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
        public string? NomeContato { get; set; }
        public string? EmailContato { get; set; }
        public StatusSugestao Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
