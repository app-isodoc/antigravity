using Isodoc.Web.Data;
using Isodoc.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isodoc.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalDocuments = await _context.Documents.CountAsync(),
            TotalNonConformities = await _context.NonConformities.CountAsync(),
            OpenNonConformities = await _context.NonConformities.CountAsync(n => n.Status == "Aberta"),
            TotalIndicators = await _context.Indicators.CountAsync(),
            TotalAudits = await _context.AuditPrograms.CountAsync()
        };

        return View(model);
    }
}

public class DashboardViewModel
{
    public int TotalDocuments { get; set; }
    public int TotalNonConformities { get; set; }
    public int OpenNonConformities { get; set; }
    public int TotalIndicators { get; set; }
    public int TotalAudits { get; set; }
}
