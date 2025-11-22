using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class Document : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(50)]
    public string Tipo { get; set; } = "Procedimento";

    [StringLength(50)]
    public string Status { get; set; } = "Rascunho";

    public Guid? PastaId { get; set; }

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
