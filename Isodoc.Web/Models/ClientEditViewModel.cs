using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class ClientEditViewModel
{
    public Guid Id { get; set; }

    // Dados da Empresa
    [Required(ErrorMessage = "A Razão Social é obrigatória")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Nome Fantasia é obrigatório")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    public string CNPJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "A URL Personalizada é obrigatória")]
    public string UrlPersonalizada { get; set; } = string.Empty;

    // Endereço
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;

    // Financeiro e Controle
    public decimal ValorMensal { get; set; }
    public DateTime? DataSuspensao { get; set; }
    public string Status { get; set; } = "Ativo";
    public string Plano { get; set; } = "Standard";
}
