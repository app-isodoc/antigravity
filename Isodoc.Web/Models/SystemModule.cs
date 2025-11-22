using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class SystemModule
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // Identificador único

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty; // Nome exibido

    [StringLength(200)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; } // Classe CSS do ícone (ex: "bi bi-file-text")

    public int Order { get; set; } = 0; // Ordem no menu

    public bool IsActive { get; set; } = true;

    public bool RequiresApproval { get; set; } = false; // Se o módulo tem funcionalidade de aprovação
}
