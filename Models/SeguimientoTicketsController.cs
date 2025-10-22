using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tickets.Filters;
using Tickets.Models;

namespace Tickets.Models
{
    [RolAuthorize("Tecnico, Cliente")]
    public class SeguimientoTicketsController : Controller
    {
        private readonly TicketsDbContext _context;

        public SeguimientoTicketsController(TicketsDbContext context)
        {
            _context = context;
        }

        // GET: SeguimientoTickets
        public async Task<IActionResult> Index()
        {
            var ticketsDbContext = _context.SeguimientoTickets.Include(s => s.IdTicketNavigation).Include(s => s.IdUsuarioNavigation);
            return View(await ticketsDbContext.ToListAsync());
        }

        // GET: SeguimientoTickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seguimientoTicket = await _context.SeguimientoTickets
                .Include(s => s.IdTicketNavigation)
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdSeguimiento == id);
            if (seguimientoTicket == null)
            {
                return NotFound();
            }

            return View(seguimientoTicket);
        }
        [RolAuthorize("Tecnico")]
        // GET: SeguimientoTickets/Create
        public IActionResult Create(int? ticketId)
        {
            if (ticketId == null)
            {
                // Si no viene el ID del ticket, no puede crear un seguimiento.
                return NotFound("Debe especificar el ID del Ticket al que desea dar seguimiento.");
            }

            // Usamos ViewBag para pasar el ID del ticket a la vista.
            ViewBag.IdTicket = ticketId.Value;

            // Devolvemos un objeto vacío para el formulario
            return View(new SeguimientoTicket { IdTicket = ticketId.Value });
        }

        // POST: SeguimientoTickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSeguimiento,IdTicket,IdUsuario,Mensaje")] SeguimientoTicket seguimientoTicket)
        {
            var userIdString = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError(string.Empty, "Error de Sesión: No se pudo identificar al usuario.");
            }
            else
            {
                seguimientoTicket.IdUsuario = userId;
                seguimientoTicket.Fecha = DateTime.Now; // Aseguramos que la fecha esté asignada
            }

            // Si la ID del Ticket que viene del formulario es 0 o inválida, esto fallará.
            if (seguimientoTicket.IdTicket <= 0 || !_context.Tickets.Any(t => t.IdTicket == seguimientoTicket.IdTicket))
            {
                ModelState.AddModelError("IdTicket", "El ID del Ticket no es válido o no existe.");
            }


            if (ModelState.IsValid)
            {
                // El código solo llega aquí si la ID de sesión es correcta, el ticket existe, y Mensaje no está vacío.
                try
                {
                    _context.Add(seguimientoTicket);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Tickets", new { id = seguimientoTicket.IdTicket });
                }
                catch (Exception ex)
                {
                    // CAPTURA cualquier error de base de datos/inserción aquí (por si el problema es la DB)
                    ModelState.AddModelError(string.Empty, $"Error al guardar en DB: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Error de DB: {ex.ToString()}");
                }
            }

            // Si llegamos aquí, ModelState.IsValid falló o la DB falló.

            // --- NUEVO PARA DEPURACIÓN ---
            // Recorre los errores y muéstralos en la ventana de Salida.
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Error de Validación: Campo '{state.Key}' - Mensaje: {error.ErrorMessage}");
                }
            }
            // --- FIN DEPURACIÓN ---

            // Recargar ViewBag
            ViewBag.IdTicket = seguimientoTicket.IdTicket;
            return View(seguimientoTicket);
        }
        [RolAuthorize("Tecnico")]
        // GET: SeguimientoTickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seguimientoTicket = await _context.SeguimientoTickets.FindAsync(id);
            if (seguimientoTicket == null)
            {
                return NotFound();
            }
            ViewData["IdTicket"] = new SelectList(_context.Tickets, "IdTicket", "IdTicket", seguimientoTicket.IdTicket);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", seguimientoTicket.IdUsuario);
            return View(seguimientoTicket);
        }

        // POST: SeguimientoTickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSeguimiento,IdTicket,IdUsuario,Mensaje,Fecha")] SeguimientoTicket seguimientoTicket)
        {
            if (id != seguimientoTicket.IdSeguimiento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seguimientoTicket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeguimientoTicketExists(seguimientoTicket.IdSeguimiento))
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
            ViewData["IdTicket"] = new SelectList(_context.Tickets, "IdTicket", "IdTicket", seguimientoTicket.IdTicket);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", seguimientoTicket.IdUsuario);
            return View(seguimientoTicket);
        }
        [RolAuthorize("Tecnico")]
        // GET: SeguimientoTickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seguimientoTicket = await _context.SeguimientoTickets
                .Include(s => s.IdTicketNavigation)
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdSeguimiento == id);
            if (seguimientoTicket == null)
            {
                return NotFound();
            }

            return View(seguimientoTicket);
        }

        // POST: SeguimientoTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seguimientoTicket = await _context.SeguimientoTickets.FindAsync(id);
            if (seguimientoTicket != null)
            {
                _context.SeguimientoTickets.Remove(seguimientoTicket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeguimientoTicketExists(int id)
        {
            return _context.SeguimientoTickets.Any(e => e.IdSeguimiento == id);
        }
    }
}
