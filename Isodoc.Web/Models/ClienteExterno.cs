using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class ClienteExterno : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(18)]
    public string? CNPJ { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(100)]
    public string? Cidade { get; set; }

    [StringLength(2)]
    public string? Estado { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
}

public class Departamento : IMustHaveTenant
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Responsavel { get; set; }

    [StringLength(200)]
    public string? Descricao { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
}
