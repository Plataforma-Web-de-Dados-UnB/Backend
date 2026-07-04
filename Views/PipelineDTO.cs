using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Views
{
    public class PipelineCreateDto
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        public string ScriptPython { get; set; } = string.Empty;
    }

    public class PipelineUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        public string ScriptPython { get; set; } = string.Empty;
    }

    public class PipelineGetDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string ScriptPython { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PipelineListDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PipelineExecucaoCreateDto
    {
        [Required]
        public Guid BatchId { get; set; }

        [Required]
        public int PipelineId { get; set; }

        [Required]
        [StringLength(255)]
        public string TabelaSilver { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string TabelaGold { get; set; } = string.Empty;
    }

    public class PipelineExecucaoGetDto
    {
        public int Id { get; set; }
        public Guid BatchId { get; set; }
        public int PipelineId { get; set; }
        public string PipelineNome { get; set; } = string.Empty;
        public string TabelaSilver { get; set; } = string.Empty;
        public string TabelaGold { get; set; } = string.Empty;
        public StatusPipelineExecucao Status { get; set; }
        public string? Mensagem { get; set; }
        public DateTime? IniciadoEm { get; set; }
        public DateTime? FinalizadoEm { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PipelineExecucaoListDto
    {
        public int Id { get; set; }
        public Guid BatchId { get; set; }
        public int PipelineId { get; set; }
        public string PipelineNome { get; set; } = string.Empty;
        public string TabelaSilver { get; set; } = string.Empty;
        public string TabelaGold { get; set; } = string.Empty;
        public StatusPipelineExecucao Status { get; set; }
        public DateTime? FinalizadoEm { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UploadRequestDto
    {
        [Required]
        public IFormFile Arquivo { get; set; } = null!;
    }

    public class UploadPreviewDto
    {
        public Guid BatchId { get; set; }
        public List<string> Colunas { get; set; } = [];
        public int TotalLinhas { get; set; }
        public List<Dictionary<string, string>> PrimeirasLinhas { get; set; } = [];
    }
}
