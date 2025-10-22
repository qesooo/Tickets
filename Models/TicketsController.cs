using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tickets.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp; 
using MailKit.Security; 
using MimeKit; 

namespace Tickets.Models
{
    [RolAuthorize("Tecnico, Cliente")]
    public class TicketsController : Controller
    {
        private readonly TicketsDbContext _context;
        private readonly IConfiguration _configuration;

        public TicketsController(TicketsDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            // 1. Obtener el rol y la ID del usuario logueado
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var userIdString = HttpContext.Session.GetString("UsuarioId");

            // Validar y parsear la ID de usuario. 
            // ESTO ES CLAVE para declarar 'userId' y manejar errores.
            if (string.IsNullOrEmpty(rol) || string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                // En un caso real, esto debería redirigir a Login o a una vista de error.
                // Asumimos que la sesión es inválida aquí.
                return Unauthorized("Error de Sesión: No se pudo identificar al usuario o su rol/ID es inválida.");
            }

            // 2. Base de datos con las inclusiones necesarias
            var ticketsQuery = _context.Tickets
                .Include(t => t.IdClienteNavigation)
                .Include(t => t.IdTecnicoNavigation)
                .AsQueryable();


            // 3. Aplicar el filtro según el rol
            if (rol == "Tecnico")
            {
                // CORRECCIÓN: Usamos la variable local 'userId' (que es un int) en lugar de la inexistente 'IdUsuario'
                ticketsQuery = ticketsQuery.Where(t => t.IdTecnico == userId);
                ViewData["IsTecnico"] = true;
            }
            else if (rol == "Cliente")
            {
                // CORRECCIÓN: Usamos la variable local 'userId' (que es la ID del cliente) en lugar de 'clienteNombre'
                ticketsQuery = ticketsQuery.Where(t => t.IdCliente == userId);
                ViewData["IsTecnico"] = false;
            }
            else
            {
                // Rol no reconocido o Admin (ve todos por defecto si no se aplica filtro de seguridad extra)
                ViewData["IsTecnico"] = false;
            }


            // 4. Ejecutar la consulta
            var tickets = await ticketsQuery.ToListAsync();

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.IdClienteNavigation)
                .Include(t => t.IdTecnicoNavigation)
                .FirstOrDefaultAsync(m => m.IdTicket == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
        [RolAuthorize("Cliente")]
        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["IdTecnico"] = new SelectList(_context.Usuarios.Where(u => u.Rol == "Tecnico"), "IdUsuario", "Nombre");
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTicket,IdCliente,IdTecnico,Asunto,Texto,Prioridad,FechaCierre")] Ticket ticket)
        {
            var clienteNombre = HttpContext.Session.GetString("UsuarioNombre");

            if (string.IsNullOrEmpty(clienteNombre))
            {
                ModelState.AddModelError(string.Empty, "No se pudo obtener el nombre de usuario de la sesión.");
            }
            else
            {
                var cliente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nombre == clienteNombre);
                if (cliente != null)
                {
                    ticket.IdCliente = cliente.IdUsuario;
                    ticket.FechaCreacion = DateTime.Now;
                    ticket.Estado = "Abierto";
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error: No se pudo identificar al cliente.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync(); // <-- El ticket se guarda aquí

                // --- INICIO DE LÓGICA DE CORREO ---

                if (ticket.IdTecnico.HasValue) // Verificamos si se asignó un técnico
                {
                    // Buscamos los datos del técnico
                    var tecnico = await _context.Usuarios.FindAsync(ticket.IdTecnico.Value);

                    if (tecnico != null && !string.IsNullOrEmpty(tecnico.Correo))
                    {
                        try
                        {
                            // 1. Leer la configuración de appsettings.json
                            var config = _configuration.GetSection("SmtpSettings");
                            var smtpServer = config["Server"];
                            var smtpPort = int.Parse(config["Port"]);
                            var senderEmail = config["SenderEmail"];
                            var senderName = config["SenderName"];
                            var username = config["Username"];
                            var password = config["Password"];

                            // 2. Crear el mensaje
                            var message = new MimeMessage();
                            message.From.Add(new MailboxAddress(senderName, senderEmail));
                            message.To.Add(new MailboxAddress(tecnico.Nombre, tecnico.Correo));
                            message.Subject = $"Nuevo Ticket Asignado: #{ticket.IdTicket} - {ticket.Asunto}";

                            message.Body = new TextPart("html")
                            {
                                Text = $@"
                                    <h3>¡Hola {tecnico.Nombre}!</h3>
                                    <p>Se te ha asignado un nuevo ticket de soporte.</p>
                                    <ul>
                                        <li><b>Ticket ID:</b> {ticket.IdTicket}</li>
                                        <li><b>Cliente:</b> {clienteNombre}</li>
                                        <li><b>Asunto:</b> {ticket.Asunto}</li>
                                        <li><b>Prioridad:</b> {ticket.Prioridad}</li>
                                    </ul>
                                    <p>Por favor, revísalo en el sistema.</p>"
                            };

                            // 3. Enviar el correo
                            using (var client = new SmtpClient())
                            {
                                // Conectar al servidor SMTP
                                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.SslOnConnect);

                                // Autenticarse
                                await client.AuthenticateAsync(username, password);

                                // Enviar
                                await client.SendAsync(message);

                                // Desconectar
                                await client.DisconnectAsync(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Opcional: Escribe en la ventana de Salida de Visual Studio
                            System.Diagnostics.Debug.WriteLine($"Error de Correo: {ex.Message}");
                        }
                    }
                }
                // --- FIN DE LÓGICA DE CORREO ---

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdTecnico"] = new SelectList(_context.Usuarios.Where(u => u.Rol == "Tecnico"), "IdUsuario", "Nombre", ticket.IdTecnico);
            return View(ticket);
        }
        [RolAuthorize("Cliente")]
        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["IdCliente"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre", ticket.IdCliente);
            ViewData["IdTecnico"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre", ticket.IdTecnico);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTicket,IdCliente,IdTecnico,Asunto,Texto,Estado,Prioridad,FechaCreacion,FechaCierre")] Ticket ticket)
        {
            if (id != ticket.IdTicket)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.IdTicket))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCliente"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre", ticket.IdCliente);
            ViewData["IdTecnico"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre", ticket.IdTecnico);
            return View(ticket);
        }
        [RolAuthorize("Cliente")]
        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.IdClienteNavigation)
                .Include(t => t.IdTecnicoNavigation)
                .FirstOrDefaultAsync(m => m.IdTicket == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.IdTicket == id);
        }
    }
}
