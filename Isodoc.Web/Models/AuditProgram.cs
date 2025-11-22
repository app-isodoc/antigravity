using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class AuditProgram : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    public DateTime? Cronograma { get; set; }

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
}
