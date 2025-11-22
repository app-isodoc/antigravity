using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isodoc.Web.Controllers;

[AllowAnonymous]
public class LandingController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult SolicitarDemo()
    {
        // Redirecionar para formulário ou WhatsApp
        return Redirect("https://wa.me/5511972070613?text=Olá! Gostaria de solicitar uma demonstração do Isodoc.");
    }

    public IActionResult FalarVendas()
    {
        // Redirecionar para WhatsApp
        return Redirect("https://wa.me/5511972070613?text=Olá! Gostaria de falar sobre o Isodoc.");
    }
}
