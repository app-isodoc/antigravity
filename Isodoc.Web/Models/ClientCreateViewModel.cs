using System.ComponentModel.DataAnnotations;

namespace Isodoc.Web.Models;

public class ClientCreateViewModel
{
    // Dados da Empresa
    [Required(ErrorMessage = "A Razão Social é obrigatória")]
    [StringLength(200, ErrorMessage = "A Razão Social deve ter no máximo 200 caracteres")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Nome Fantasia é obrigatório")]
    [StringLength(100, ErrorMessage = "O Nome Fantasia deve ter no máximo 100 caracteres")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [StringLength(18, ErrorMessage = "O CNPJ deve ter no máximo 18 caracteres")]
    public string CNPJ { get; set; } = string.Empty;

    [Required(ErrorMessage = "A URL Personalizada é obrigatória")]
    [StringLength(100, ErrorMessage = "A URL Personalizada deve ter no máximo 100 caracteres")]
    public string UrlPersonalizada { get; set; } = string.Empty;

    // Endereço
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;

    // Contatos (O primeiro será o Master)
    public List<ContactViewModel> Contatos { get; set; } = new List<ContactViewModel>();

    // Financeiro e Controle removidos temporariamente
}

public class ContactViewModel
{
    [Required(ErrorMessage = "O Nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;
    
    public string Telefone { get; set; } = string.Empty;
    public bool IsMaster { get; set; }
}
