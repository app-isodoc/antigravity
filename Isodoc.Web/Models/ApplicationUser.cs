using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string? Nome { get; set; }
    
    public bool Ativo { get; set; } = true;

    public Guid? ClientId { get; set; }
    
    // Navegação
    public Client? Client { get; set; }

    // Informações Profissionais
    [StringLength(100)]
    public string? Funcao { get; set; } // Cargo na empresa

    [StringLength(100)]
    public string? Departamento { get; set; }

    // Controle de Acesso
    public bool IsMaster { get; set; } = false; // Usuário master da empresa
    
    public UserRole Role { get; set; } = UserRole.Usuario;

    public bool PrimeiroAcesso { get; set; } = true; // Forçar troca de senha

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    public string? TelefoneContato { get; set; }

    public string? Telefone { get; set; }
}

public enum UserRole
{
    Usuario = 0,
    Administrador = 1,
    Seguidor = 2,
    Master = 3,
    SuperAdmin = 99
}
