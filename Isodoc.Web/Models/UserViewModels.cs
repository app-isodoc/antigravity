namespace Isodoc.Web.Models;

public class UserCreateViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Funcao { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<Guid> SelectedModules { get; set; } = new();
    public string? Telefone { get; set; }
    public int Role { get; set; }
    public string? Departamento { get; set; }
    public bool EnviarCredenciais { get; set; }
}

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Funcao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public List<Guid> SelectedModules { get; set; } = new();
    public string? Telefone { get; set; }
    public int Role { get; set; }
    public string? Departamento { get; set; }
    public System.Collections.Generic.IList<string>? Permissions { get; set; }
}
