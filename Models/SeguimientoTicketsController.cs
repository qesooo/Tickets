using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Models
{
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

        // GET: SeguimientoTickets/Create
        public IActionResult Create()
        {
            ViewData["IdTicket"] = new SelectList(_context.Tickets, "IdTicket", "IdTicket");
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario");
            return View();
        }

        // POST: SeguimientoTickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSeguimiento,IdTicket,IdUsuario,Mensaje,Fecha")] SeguimientoTicket seguimientoTicket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seguimientoTicket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdTicket"] = new SelectList(_context.Tickets, "IdTicket", "IdTicket", seguimientoTicket.IdTicket);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", seguimientoTicket.IdUsuario);
            return View(seguimientoTicket);
        }

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
