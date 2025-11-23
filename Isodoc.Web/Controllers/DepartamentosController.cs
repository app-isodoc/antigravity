using Isodoc.Web.Data;
using Isodoc.Web.Models;
using Isodoc.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isodoc.Web.Controllers;

[Authorize]
public class DepartamentosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public DepartamentosController(ApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    // GET: Departamentos
    public async Task<IActionResult> Index()
    {
        var departamentos = await _context.Departamentos
            .OrderBy(d => d.Nome)
            .ToListAsync();

        return View(departamentos);
    }

    // GET: Departamentos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Departamentos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Departamento model)
    {
        if (ModelState.IsValid)
        {
            model.Id = Guid.NewGuid();
            model.ClientId = _tenantService.ClientId ?? Guid.Empty;
            model.DataCadastro = DateTime.UtcNow;
            model.Ativo = true;

            _context.Departamentos.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Departamento cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: Departamentos/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento == null)
        {
            return NotFound();
        }

        return View(departamento);
    }

    // POST: Departamentos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Departamento model)
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

                TempData["Success"] = "Departamento atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DepartamentoExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        return View(model);
    }

    // POST: Departamentos/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var departamento = await _context.Departamentos.FindAsync(id);
        if (departamento != null)
        {
            departamento.Ativo = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Departamento desativado com sucesso!";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> DepartamentoExists(Guid id)
    {
        return await _context.Departamentos.AnyAsync(e => e.Id == id);
    }
}
