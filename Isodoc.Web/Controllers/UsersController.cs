using Isodoc.Web.Models;
using Isodoc.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Isodoc.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentTenantService _currentTenantService;

    public UsersController(UserManager<ApplicationUser> userManager, ICurrentTenantService currentTenantService)
    {
        _userManager = userManager;
        _currentTenantService = currentTenantService;
    }

    public async Task<IActionResult> Index(string? search, string? roleFilter, string? statusFilter)
    {
        var query = _userManager.Users.AsQueryable();

        // O filtro de tenant já é aplicado globalmente pelo EF Core via ApplicationDbContext
        // Mas como UserManager pode ignorar filtros globais em alguns casos, vamos reforçar
        if (!_currentTenantService.IsSuperAdmin && _currentTenantService.ClientId.HasValue)
        {
            query = query.Where(u => u.ClientId == _currentTenantService.ClientId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Nome.Contains(search) || u.Email.Contains(search));
        }

        if (!string.IsNullOrEmpty(roleFilter) && Enum.TryParse<UserRole>(roleFilter, out var role))
        {
            query = query.Where(u => u.Role == role);
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            bool isActive = statusFilter == "Ativo";
            query = query.Where(u => u.Ativo == isActive);
        }

        var users = await query.ToListAsync();
        
        ViewBag.Search = search;
        ViewBag.RoleFilter = roleFilter;
        ViewBag.StatusFilter = statusFilter;

        return View(users);
    }
}

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
