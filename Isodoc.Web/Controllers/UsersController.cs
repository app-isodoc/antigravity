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
    private readonly IEmailService _emailService;
    private readonly Isodoc.Web.Data.ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, ICurrentTenantService currentTenantService, IEmailService emailService, Isodoc.Web.Data.ApplicationDbContext context)
    {
        _userManager = userManager;
        _currentTenantService = currentTenantService;
        _emailService = emailService;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? roleFilter, string? statusFilter)
    {
        var query = _userManager.Users.AsQueryable();

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

    // GET: Users/Create
    public async Task<IActionResult> Create()
    {
        var departamentos = await _context.Departamentos
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();

        ViewBag.Departamentos = departamentos;
        return View();
    }

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Verificar se email já existe
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Este email já está cadastrado no sistema.");
                
                var departamentos = await _context.Departamentos
                    .Where(d => d.Ativo)
                    .OrderBy(d => d.Nome)
                    .ToListAsync();
                ViewBag.Departamentos = departamentos;
                
                return View(model);
            }

            // Gerar senha provisória se não informada (ou sempre gerar para garantir segurança)
            string senhaProvisoria = GerarSenhaProvisoria();

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                Funcao = model.Funcao,
                Telefone = model.Telefone,
                Departamento = model.Departamento,
                Role = (UserRole)model.Role,
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                PrimeiroAcesso = true,
                EmailConfirmed = true, // Confirmamos automaticamente pois o admin criou
                ClientId = _currentTenantService.ClientId
            };

            var result = await _userManager.CreateAsync(user, senhaProvisoria);

            if (result.Succeeded)
            {
                // Enviar email de boas-vindas
                try 
                {
                    var client = await _context.Clients.FindAsync(_currentTenantService.ClientId);
                    string clientName = client?.NomeFantasia ?? "Isodoc";
                    string loginUrl = Url.Action("Login", "Account", null, Request.Scheme) ?? "https://isodoc.com.br/login";

                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Nome, clientName, senhaProvisoria, loginUrl);
                    TempData["Success"] = "Usuário criado com sucesso! As credenciais foram enviadas por email.";
                }
                catch (Exception ex)
                {
                    // Logar erro mas não impedir o cadastro
                    TempData["Warning"] = "Usuário criado, mas houve erro ao enviar o email: " + ex.Message;
                }

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Se chegou aqui, algo deu errado
        var deptos = await _context.Departamentos
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();
        ViewBag.Departamentos = deptos;

        return View(model);
    }

    private string GerarSenhaProvisoria()
    {
        const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10)
            .Select(s => s[random.Next(s.Length)]).ToArray());
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
