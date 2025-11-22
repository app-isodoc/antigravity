using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class NonConformity : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = "Aberta";

    [StringLength(50)]
    public string Tipo { get; set; } = "Interna";

    public Guid? OrigemId { get; set; }

    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? Prazo { get; set; }

    [StringLength(100)]
    public string EtapaAtual { get; set; } = "Identificação";

    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
}
