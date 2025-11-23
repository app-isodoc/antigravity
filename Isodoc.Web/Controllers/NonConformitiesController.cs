using Isodoc.Web.Data;
using Isodoc.Web.Models;
using Isodoc.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isodoc.Web.Controllers;

[Authorize]
public class NonConformitiesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentTenantService _tenantService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public NonConformitiesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICurrentTenantService tenantService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _tenantService = tenantService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    // ... (código existente) ...

    private async Task NotificarResponsaveis(NonConformity nc)
    {
        var responsaveis = new List<(string? Id, string Role)>
        {
            (nc.ResponsavelContencaoId, "Ações de Contenção"),
            (nc.ResponsavelAnaliseCausaId, "Análise de Causa"),
            (nc.ResponsavelAcaoCorretivaId, "Ações Corretivas"),
            (nc.ResponsavelVerificacaoId, "Verificação da Eficácia")
        };

        foreach (var resp in responsaveis)
        {
            if (!string.IsNullOrEmpty(resp.Id))
            {
                var user = await _userManager.FindByIdAsync(resp.Id);
                if (user != null)
                {
                    // Enviar E-mail
                    string subject = $"[ISODOC] Você foi designado para: {resp.Role} - {nc.Numero}";
                    string body = $@"
                        <h2>Nova Atribuição de Não Conformidade</h2>
                        <p>Olá <strong>{user.Nome}</strong>,</p>
                        <p>Você foi designado como responsável pela etapa de <strong>{resp.Role}</strong> na Não Conformidade <strong>{nc.Numero}</strong>.</p>
                        <p><strong>Título:</strong> {nc.Titulo}</p>
                        <p><strong>Descrição:</strong> {nc.Descricao}</p>
                        <p>Por favor, acesse o sistema para tomar as providências necessárias.</p>
                        <br>
                        <p>Atenciosamente,<br>Equipe ISODOC</p>";

                    try 
                    {
                        await _emailService.SendEmailAsync(user.Email, subject, body, user.Nome);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar email para {user.Email}: {ex.Message}");
                    }

                    // Criar Notificação no Sistema ("Sininho")
                    string notifTitle = $"Nova Atribuição: {resp.Role}";
                    string notifMessage = $"NC {nc.Numero}: {nc.Titulo}";
                    string notifLink = Url.Action("Edit", "NonConformities", new { id = nc.Id }) ?? "#";

                    await _notificationService.CreateNotificationAsync(user.Id, notifTitle, notifMessage, notifLink);
                }
            }
        }
    }

    // GET: NonConformities
    public async Task<IActionResult> Index(string? status, string? etapa)
    {
        var query = _context.NonConformities
            .Include(n => n.CreatedBy)
            .Include(n => n.ResponsavelContencao)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(n => n.Status == status);
        }

        if (!string.IsNullOrEmpty(etapa))
        {
            query = query.Where(n => n.EtapaAtual == etapa);
        }

        var nonConformities = await query
            .OrderByDescending(n => n.DataRegistro)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.EtapaFilter = etapa;

        return View(nonConformities);
    }

    // GET: NonConformities/Create
    public async Task<IActionResult> Create()
    {
        var users = await _userManager.Users
            .Where(u => u.ClientId == _tenantService.ClientId)
            .ToListAsync();

        var clientes = await _context.ClientesExternos
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        var departamentos = await _context.Departamentos
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();

        ViewBag.Users = users;
        ViewBag.Clientes = clientes;
        ViewBag.Departamentos = departamentos;
        
        return View();
    }

    // POST: NonConformities/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NonConformity model, List<IFormFile> evidencias)
    {
        // Remover campos controlados pelo sistema da validação
        ModelState.Remove("Numero");
        ModelState.Remove("Client");
        ModelState.Remove("CreatedBy");
        ModelState.Remove("ClientId");
        ModelState.Remove("CreatedById");
        ModelState.Remove("Status");
        ModelState.Remove("EtapaAtual");
        ModelState.Remove("Progresso");
        ModelState.Remove("DataRegistro");

        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            // Gerar número automático
            var year = DateTime.Now.Year;
            var count = await _context.NonConformities
                .Where(n => n.DataRegistro.Year == year)
                .CountAsync();
            
            model.Id = Guid.NewGuid();
            model.Numero = $"NC-{year}-{(count + 1):D4}";
            model.DataRegistro = DateTime.UtcNow;
            model.CreatedById = currentUser?.Id;
            model.ClientId = _tenantService.ClientId ?? Guid.Empty;
            model.Status = "Aberta";
            model.EtapaAtual = "Identificação";
            model.Progresso = 20;

            _context.Add(model);
            await _context.SaveChangesAsync();

            // Processar Evidências Iniciais
            if (evidencias != null && evidencias.Count > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "evidencias");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var file in evidencias)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var evidence = new NonConformityEvidence
                        {
                            Id = Guid.NewGuid(),
                            NonConformityId = model.Id,
                            Descricao = "Evidência Inicial",
                            ArquivoNome = file.FileName,
                            ArquivoUrl = $"/uploads/evidencias/{fileName}",
                            DataUpload = DateTime.UtcNow,
                            UploadedById = currentUser?.Id
                        };

                        _context.NonConformityEvidences.Add(evidence);
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Notificar responsáveis
            await NotificarResponsaveis(model);

            TempData["Success"] = "Não conformidade criada com sucesso! Os responsáveis foram notificados.";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        else
        {
            // Log de erros de validação para debug
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Erro de Validação: {error.ErrorMessage}");
                }
            }
        }

        var users = await _userManager.Users
            .Where(u => u.ClientId == _tenantService.ClientId)
            .ToListAsync();

        var clientes = await _context.ClientesExternos
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        var departamentos = await _context.Departamentos
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();

        ViewBag.Users = users;
        ViewBag.Clientes = clientes;
        ViewBag.Departamentos = departamentos;

        return View(model);
    }



    // GET: NonConformities/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var nonConformity = await _context.NonConformities
            .Include(n => n.Evidencias)
            .Include(n => n.Acoes)
            .ThenInclude(a => a.Responsavel)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nonConformity == null)
        {
            return NotFound();
        }

        var users = await _userManager.Users
            .Where(u => u.ClientId == _tenantService.ClientId)
            .ToListAsync();

        ViewBag.Users = users;
        return View(nonConformity);
    }

    // POST: NonConformities/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, NonConformity model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        // Remover validações de campos que não são editados aqui ou são controlados
        ModelState.Remove("Numero");
        ModelState.Remove("Client");
        ModelState.Remove("CreatedBy");
        ModelState.Remove("ClientId");
        ModelState.Remove("CreatedById");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Não conformidade atualizada com sucesso!";
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await NonConformityExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        var users = await _userManager.Users
            .Where(u => u.ClientId == _tenantService.ClientId)
            .ToListAsync();

        ViewBag.Users = users;
        return View(model);
    }

    // POST: NonConformities/AddEvidence
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvidence(Guid nonConformityId, string descricao)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var evidence = new NonConformityEvidence
        {
            Id = Guid.NewGuid(),
            NonConformityId = nonConformityId,
            Descricao = descricao,
            DataUpload = DateTime.UtcNow,
            UploadedById = currentUser?.Id
        };

        _context.NonConformityEvidences.Add(evidence);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Evidência adicionada com sucesso!";
        return RedirectToAction(nameof(Edit), new { id = nonConformityId });
    }

    // POST: NonConformities/AddAction
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAction(Guid nonConformityId, string tipo, string descricao, string? responsavelId, string? departamento, DateTime? prazo)
    {
        var action = new NonConformityAction
        {
            Id = Guid.NewGuid(),
            NonConformityId = nonConformityId,
            Tipo = tipo,
            Descricao = descricao,
            ResponsavelId = responsavelId,
            Departamento = departamento,
            Prazo = prazo,
            Status = "Pendente"
        };

        _context.NonConformityActions.Add(action);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Ação adicionada com sucesso!";
        return RedirectToAction(nameof(Edit), new { id = nonConformityId });
    }

    // POST: NonConformities/UpdateStage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStage(Guid id, string etapa)
    {
        var nc = await _context.NonConformities.FindAsync(id);
        if (nc == null)
        {
            return NotFound();
        }

        nc.EtapaAtual = etapa;
        
        // Atualizar progresso baseado na etapa
        nc.Progresso = etapa switch
        {
            "Identificação" => 20,
            "Ações de Contenção" => 40,
            "Análise de Causa" => 60,
            "Ações Corretivas" => 80,
            "Verificação" => 100,
            _ => nc.Progresso
        };

        if (etapa == "Verificação" && nc.EficazVerificacao == true)
        {
            nc.Status = "Encerrada";
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Etapa atualizada para: {etapa}";
        return RedirectToAction(nameof(Edit), new { id });
    }

    private async Task<bool> NonConformityExists(Guid id)
    {
        return await _context.NonConformities.AnyAsync(e => e.Id == id);
    }
}
