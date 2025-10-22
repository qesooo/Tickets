using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Tickets.Filters
{
    public class RolAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _rolesPermitidos;

        public RolAuthorizeAttribute(string rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos.Split(',').Select(r => r.Trim()).ToArray();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var rol = context.HttpContext.Session.GetString("UsuarioRol");

            if (rol == null || !_rolesPermitidos.Contains(rol))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
