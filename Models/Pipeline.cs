using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public enum StatusPipelineExecucao
{
    Pendente,
    Processando,
    Sucesso,
    Erro,
    Cancelado,
    Rollback
}

[Table("pipelines", Schema = "pipeline")]
public class Pipeline
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Required]
    [Column("script_python")]
    public string ScriptPython { get; set; } = string.Empty;

    [Required]
    [Column("ativo")]
    public bool Ativo { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("execucoes", Schema = "pipeline")]
public class PipelineExecucao
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("batch_id")]
    public Guid BatchId { get; set; }

    [Required]
    [Column("pipeline_id")]
    public int PipelineId { get; set; }

    [ForeignKey("PipelineId")]
    public Pipeline Pipeline { get; set; } = null!;

    [Required]
    [StringLength(255)]
    [Column("tabela_silver")]
    public string TabelaSilver { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("tabela_gold")]
    public string TabelaGold { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    [EnumDataType(typeof(StatusPipelineExecucao))]
    public StatusPipelineExecucao Status { get; set; } = StatusPipelineExecucao.Pendente;

    [Column("mensagem")]
    public string? Mensagem { get; set; }

    [Column("iniciado_em")]
    public DateTime? IniciadoEm { get; set; }

    [Column("finalizado_em")]
    public DateTime? FinalizadoEm { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("arquivos_brutos", Schema = "bronze")]
public class BronzeArquivoBruto
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("batch_id")]
    public Guid BatchId { get; set; }

    [Required]
    [Column("numero_linha")]
    public int NumeroLinha { get; set; }

    [Required]
    [Column("dados", TypeName = "jsonb")]
    public string Dados { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("uploads_auditoria", Schema = "bronze")]
public class BronzeUploadAuditoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("file_hash")]
    public string FileHash { get; set; } = string.Empty;

    [Required]
    [Column("batch_id")]
    public Guid BatchId { get; set; }

    [Required]
    [Column("nome_arquivo")]
    public string NomeArquivo { get; set; } = string.Empty;

    [Column("total_linhas")]
    public int TotalLinhas { get; set; }

    [Column("uploaded_by")]
    public string? UploadedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
