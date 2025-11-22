using Isodoc.Web.Data;
using Isodoc.Web.Models;
using Isodoc.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isodoc.Web.Controllers;

public class AdminSaaSController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public AdminSaaSController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Clients()
    {
        var clients = await _context.Clients.ToListAsync();
        return View(clients);
    }

    [HttpGet]
    public IActionResult CreateClient()
    {
        var model = new ClientCreateViewModel();
        // Inicializa com um contato para o usuário master
        model.Contatos.Add(new ContactViewModel { IsMaster = true });
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(ClientCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Criar o Cliente
                var client = new Client
                {
                    Id = Guid.NewGuid(),
                    RazaoSocial = model.RazaoSocial,
                    NomeFantasia = model.NomeFantasia,
                    CNPJ = model.CNPJ,
                    UrlPersonalizada = model.UrlPersonalizada,
                    Logradouro = model.Logradouro,
                    Numero = model.Numero,
                    Complemento = model.Complemento,
                    Bairro = model.Bairro,
                    Cidade = model.Cidade,
                    Estado = model.Estado,
                    CEP = model.CEP,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Ativo"
                };

                // Pegar o contato master (primeiro da lista ou marcado como master)
                var masterContact = model.Contatos.FirstOrDefault(c => c.IsMaster) ?? model.Contatos.FirstOrDefault();
                
                if (masterContact != null)
                {
                    client.EmailEmpresa = masterContact.Email;
                    client.Telefone = masterContact.Telefone;
                }

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                // 2. Criar o Usuário Master
                if (masterContact != null)
                {
                    var masterUser = new ApplicationUser
                    {
                        UserName = masterContact.Email,
                        Email = masterContact.Email,
                        Nome = masterContact.Nome,
                        ClientId = client.Id,
                        Role = UserRole.Master,
                        Ativo = true,
                        EmailConfirmed = true,
                        Funcao = masterContact.Cargo,
                        Telefone = masterContact.Telefone,
                        IsMaster = true
                    };

                    // Gerar uma senha provisória forte
                    string tempPassword = "Isodoc" + DateTime.Now.Year + "!"; 
                    
                    var result = await _userManager.CreateAsync(masterUser, tempPassword);
                    if (result.Succeeded)
                    {
                        client.MasterUserId = masterUser.Id;
                        _context.Clients.Update(client);
                        await _context.SaveChangesAsync();
                        
                        // Enviar email de boas-vindas com a senha
                        var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);
                        await _emailService.SendWelcomeEmailAsync(masterUser.Email, masterUser.Nome, client.NomeFantasia, tempPassword, loginUrl ?? "https://localhost:5001");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Erro ao criar usuário master: {error.Description}");
                        }
                        throw new Exception("Falha ao criar usuário master");
                    }
                }

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Clients));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Erro ao salvar cliente: {ex.Message}");
            }
        }
        
        // Se algo deu errado, garantir que tenha pelo menos um contato na lista para não dar erro na view
        if (!model.Contatos.Any())
        {
            model.Contatos.Add(new ContactViewModel { IsMaster = true });
        }
        
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendWelcomeEmail(Guid clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null)
        {
            return NotFound();
        }

        if (client.MasterUserId == null)
        {
            TempData["Error"] = "Este cliente não possui um usuário Master vinculado.";
            return RedirectToAction(nameof(Clients));
        }

        var masterUser = await _userManager.FindByIdAsync(client.MasterUserId);
        if (masterUser == null)
        {
            TempData["Error"] = "Usuário Master não encontrado.";
            return RedirectToAction(nameof(Clients));
        }

        // Gerar nova senha provisória
        string tempPassword = "Isodoc" + DateTime.Now.Year + "!";
        
        // Redefinir a senha
        var token = await _userManager.GeneratePasswordResetTokenAsync(masterUser);
        var result = await _userManager.ResetPasswordAsync(masterUser, token, tempPassword);

        if (result.Succeeded)
        {
            // Enviar email
            var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);
            var emailSent = await _emailService.SendWelcomeEmailAsync(masterUser.Email, masterUser.Nome, client.NomeFantasia, tempPassword, loginUrl ?? "https://localhost:5001");

            if (emailSent)
            {
                TempData["Success"] = "Senha redefinida e email reenviado com sucesso!";
            }
            else
            {
                TempData["Warning"] = "Senha redefinida, mas houve erro ao enviar o email. Verifique os logs.";
            }
        }
        else
        {
            TempData["Error"] = "Erro ao redefinir a senha do usuário.";
        }

        return RedirectToAction(nameof(Clients));
    }
}
