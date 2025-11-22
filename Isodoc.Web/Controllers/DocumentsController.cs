using Microsoft.AspNetCore.Mvc;

namespace Isodoc.Web.Controllers;

public class DocumentsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
