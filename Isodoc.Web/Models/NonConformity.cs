using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Isodoc.Web.Models;

public class NonConformity : IMustHaveTenant
{
    public Guid Id { get; set; }

    // Identificação
    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Numero { get; set; } = string.Empty; // NC-2025-0001

    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? DataOcorrencia { get; set; }

    [StringLength(50)]
    public string Tipo { get; set; } = "Interna"; // Interna, Externa, Auditoria

    [StringLength(50)]
    public string Severidade { get; set; } = "Média"; // Baixa, Média, Alta, Crítica

    [StringLength(100)]
    public string? Departamento { get; set; }

    public Guid? ClienteEnvolvidoId { get; set; }
    public Client? ClienteEnvolvido { get; set; }

    [StringLength(100)]
    public string? TipoIncidente { get; set; }

    public Guid? NormaIsoId { get; set; }

    public DateTime? PrazoConclusao { get; set; }

    // Responsáveis por cada etapa (ISO 9001 Workflow)
    public string? ResponsavelContencaoId { get; set; }
    [ForeignKey("ResponsavelContencaoId")]
    public virtual ApplicationUser? ResponsavelContencao { get; set; }

    public string? ResponsavelAnaliseCausaId { get; set; }
    [ForeignKey("ResponsavelAnaliseCausaId")]
    public virtual ApplicationUser? ResponsavelAnaliseCausa { get; set; }

    public string? ResponsavelAcaoCorretivaId { get; set; }
    [ForeignKey("ResponsavelAcaoCorretivaId")]
    public virtual ApplicationUser? ResponsavelAcaoCorretiva { get; set; }

    public string? ResponsavelVerificacaoId { get; set; }
    [ForeignKey("ResponsavelVerificacaoId")]
    public virtual ApplicationUser? ResponsavelVerificacao { get; set; }

    // Status do Workflow (Controle de etapas)
    public bool ContencaoConcluida { get; set; }
    public bool AnaliseCausaConcluida { get; set; }
    public bool AcaoCorretivaConcluida { get; set; }
    public bool VerificacaoConcluida { get; set; }

    // Ações de Contenção
    public string? AcoesContencao { get; set; }
    public DateTime? DataContencao { get; set; }

    // Análise de Causa
    [StringLength(50)]
    public string? MetodologiaAnalise { get; set; } // 5 Porquês, Ishikawa, etc.
    
    public string? AnaliseCausa { get; set; }
    public string? CausaRaiz { get; set; }
    public DateTime? DataAnalise { get; set; }

    // Ações Corretivas
    public string? AcoesCorretivas { get; set; }
    public DateTime? DataCorretivas { get; set; }

    // Verificação
    public string? ResultadoVerificacao { get; set; }
    public DateTime? DataVerificacao { get; set; }
    public bool? EficazVerificacao { get; set; }

    // Controle Geral
    [StringLength(50)]
    public string Status { get; set; } = "Aberta"; // Aberta, Em Análise, Em Implantação, Encerrada

    [StringLength(100)]
    public string EtapaAtual { get; set; } = "Identificação"; // Identificação, Contenção, Análise, Corretivas, Verificação

    public int Progresso { get; set; } = 0; // 0-100%

    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    // Relacionamentos
    public virtual ICollection<NonConformityEvidence> Evidencias { get; set; } = new List<NonConformityEvidence>();
    public virtual ICollection<NonConformityAction> Acoes { get; set; } = new List<NonConformityAction>();
}

public class NonConformityEvidence
{
    public Guid Id { get; set; }
    public Guid NonConformityId { get; set; }
    public NonConformity? NonConformity { get; set; }

    [Required]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ArquivoUrl { get; set; }

    [StringLength(100)]
    public string? ArquivoNome { get; set; }

    public DateTime DataUpload { get; set; } = DateTime.UtcNow;

    public string? UploadedById { get; set; }
    public ApplicationUser? UploadedBy { get; set; }
}

public class NonConformityAction
{
    public Guid Id { get; set; }
    public Guid NonConformityId { get; set; }
    public NonConformity? NonConformity { get; set; }

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = "Contenção"; // Contenção, Corretiva

    [Required]
    public string Descricao { get; set; } = string.Empty;

    public string? ResponsavelId { get; set; }
    public ApplicationUser? Responsavel { get; set; }

    [StringLength(100)]
    public string? Departamento { get; set; }

    public DateTime? Prazo { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pendente"; // Pendente, Em Andamento, Concluída

    public DateTime? DataConclusao { get; set; }

    public string? Observacoes { get; set; }
}
