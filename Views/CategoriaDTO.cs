using System.ComponentModel.DataAnnotations;

namespace api.Views
{
    public class CategoriaCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public IFormFile? Imagem { get; set; }

        public int SortOrdem { get; set; } = 0;
    }

    public class CategoriaUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public IFormFile? Imagem { get; set; }

        public int SortOrdem { get; set; } = 0;
    }

    public class CategoriaGetDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public int SortOrdem { get; set; }
        public bool Active { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int QuantidadePaineis { get; set; }
    }

    public class CategoriaListDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public int SortOrdem { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public int QuantidadePaineis { get; set; }
    }
}
