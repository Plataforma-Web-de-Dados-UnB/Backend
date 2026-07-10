using System.ComponentModel.DataAnnotations;

namespace api.Views
{
    public class PainelCreateDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(255, ErrorMessage = "O campo Nome deve ter no máximo 255 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required(ErrorMessage = "O link de incorporação (embed) é obrigatório.")]
        [StringLength(2048, ErrorMessage = "O link do embed deve ter no máximo 2048 caracteres.")]
        public string GraphEmbedLink { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "O UUID do embed deve ter no máximo 255 caracteres.")]
        public string? EmbedDashboardUuid { get; set; }

        public int SortOrdem { get; set; } = 0;

        [Required(ErrorMessage = "A Categoria é obrigatória.")]
        public int CategoriaId { get; set; }
    }

    public class PainelUpdateDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(255, ErrorMessage = "O campo Nome deve ter no máximo 255 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required(ErrorMessage = "O link de incorporação (embed) é obrigatório.")]
        [StringLength(2048, ErrorMessage = "O link do embed deve ter no máximo 2048 caracteres.")]
        public string GraphEmbedLink { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "O UUID do embed deve ter no máximo 255 caracteres.")]
        public string? EmbedDashboardUuid { get; set; }

        public int SortOrdem { get; set; } = 0;

        [Required(ErrorMessage = "A Categoria é obrigatória.")]
        public int CategoriaId { get; set; }
    }

    public class PainelGetDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string GraphEmbedLink { get; set; } = string.Empty;
        public string? EmbedDashboardUuid { get; set; }
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
        public string? EmbedDashboardUuid { get; set; }
        public int SortOrdem { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
    }
}
