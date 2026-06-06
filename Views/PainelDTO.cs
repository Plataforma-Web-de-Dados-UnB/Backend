using System.ComponentModel.DataAnnotations;

namespace api.Views
{
    public class PainelCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        [StringLength(2048)]
        public string GraphEmbedLink { get; set; } = string.Empty;

        public int SortOrdem { get; set; } = 0;

        [Required]
        public int CategoriaId { get; set; }
    }

    public class PainelUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        [StringLength(2048)]
        public string GraphEmbedLink { get; set; } = string.Empty;

        public int SortOrdem { get; set; } = 0;

        [Required]
        public int CategoriaId { get; set; }
    }

    public class PainelGetDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string GraphEmbedLink { get; set; } = string.Empty;
        public int SortOrdem { get; set; }
        public bool Active { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
    }

    public class PainelBuscaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
    }

    public class PainelListDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string GraphEmbedLink { get; set; } = string.Empty;
        public int SortOrdem { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
    }
}
