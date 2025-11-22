using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class Notification : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Mensagem { get; set; } = string.Empty;

    public NotificationType Tipo { get; set; } = NotificationType.Info;

    public bool Lida { get; set; } = false;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Link { get; set; } // URL para ação relacionada

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
}

public enum NotificationType
{
    Info = 0,
    Warning = 1,
    Alert = 2,
    Success = 3
}
