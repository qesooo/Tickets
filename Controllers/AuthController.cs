using Microsoft.AspNetCore.Mvc;
using Tickets.Models;
using Tickets.ViewModels;
using System.Linq;

namespace ProyectoSoporteTI.Controllers
{
    public class AuthController : Controller
    {
        private readonly TicketsDbContext _context; 

        public AuthController(TicketsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == model.Correo && u.Clave == model.Clave && u.Activo == true);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
                return View(model);
            }

            // Guarda los datos del usuario en sesión
            HttpContext.Session.SetString("UsuarioId", usuario.IdUsuario.ToString());
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol);

            // Redirigir según el rol
            if (usuario.Rol == "Tecnico")
                return RedirectToAction("IndexTecnico", "Home");
            else
                return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
