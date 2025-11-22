using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Isodoc.Web.Models;

public class Client
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required]
    [StringLength(18)]
    public string CNPJ { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string NomeFantasia { get; set; } = string.Empty;

    [StringLength(100)]
    public string UrlPersonalizada { get; set; } = string.Empty; // ex: empresa.isodoc.com.br

    // Endereço
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;

    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [StringLength(100)]
    public string EmailEmpresa { get; set; } = string.Empty;

    public string Status { get; set; } = "Ativo"; // Ativo, Inativo, Bloqueado

    public string Plano { get; set; } = "Free";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? MasterUserId { get; set; } // FK para o usuário master
    
    [StringLength(500)]
    public string? LogoUrl { get; set; }

    // Financeiro e Controle
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorMensal { get; set; }
    public DateTime? DataSuspensao { get; set; }
}
