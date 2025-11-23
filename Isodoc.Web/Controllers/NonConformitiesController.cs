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

    public NonConformitiesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICurrentTenantService tenantService)
    {
        _context = context;
        _userManager = userManager;
        _tenantService = tenantService;
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
    public async Task<IActionResult> Create(NonConformity model)
    {
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

            _context.NonConformities.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Não conformidade criada com sucesso!";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        var users = await _userManager.Users
            .Where(u => u.ClientId == _tenantService.ClientId)
            .ToListAsync();

        ViewBag.Users = users;
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
