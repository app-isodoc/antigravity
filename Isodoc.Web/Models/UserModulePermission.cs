using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class UserModulePermission : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required]
    [StringLength(100)]
    public string ModuleName { get; set; } = string.Empty;

    public bool CanView { get; set; } = false;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanApprove { get; set; } = false;

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
}
