using Microsoft.AspNetCore.Mvc;
using Tickets.Filters;

namespace Tickets.Controllers
{
    [RolAuthorize("Cliente")]
    public class ClienteController : Controller
    {
        public IActionResult Panel()
        {
            ViewBag.Usuario = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }
    }
}
