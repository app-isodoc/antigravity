using Isodoc.Web.Data;
using Isodoc.Web.Models;
using Isodoc.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isodoc.Web.Controllers;

[Authorize]
public class ClientesExternosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public ClientesExternosController(ApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    // GET: ClientesExternos
    public async Task<IActionResult> Index()
    {
        var clientes = await _context.ClientesExternos
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return View(clientes);
    }

    // GET: ClientesExternos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ClientesExternos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteExterno model)
    {
        if (ModelState.IsValid)
        {
            model.Id = Guid.NewGuid();
            model.ClientId = _tenantService.ClientId ?? Guid.Empty;
            model.DataCadastro = DateTime.UtcNow;
            model.Ativo = true;

            _context.ClientesExternos.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: ClientesExternos/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var cliente = await _context.ClientesExternos.FindAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }

        return View(cliente);
    }

    // POST: ClientesExternos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ClienteExterno model)
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

                TempData["Success"] = "Cliente atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClienteExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        return View(model);
    }

    // POST: ClientesExternos/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cliente = await _context.ClientesExternos.FindAsync(id);
        if (cliente != null)
        {
            cliente.Ativo = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cliente desativado com sucesso!";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ClienteExists(Guid id)
    {
        return await _context.ClientesExternos.AnyAsync(e => e.Id == id);
    }
}
