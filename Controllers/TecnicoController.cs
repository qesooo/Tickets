using Microsoft.AspNetCore.Mvc;
using Tickets.Filters;

namespace Tickets.Controllers
{
    [RolAuthorize("Tecnico")]
    public class TecnicoController : Controller
    {
        public IActionResult Panel()
        {
            ViewBag.Usuario = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }
    }
}
