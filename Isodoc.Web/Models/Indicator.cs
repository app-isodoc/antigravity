using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class Indicator : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }

    public ICollection<IndicatorMeasurement> Measurements { get; set; } = new List<IndicatorMeasurement>();
}

public class IndicatorMeasurement : IMustHaveTenant
{
    public Guid Id { get; set; }

    public Guid IndicatorId { get; set; }
    public Indicator? Indicator { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    public string? Notas { get; set; }

    public Guid ClientId { get; set; }
}
