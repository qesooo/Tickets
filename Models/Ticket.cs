using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tickets.Models;

public partial class Ticket
{
    public int IdTicket { get; set; }
    public int IdCliente { get; set; }
    public int? IdTecnico { get; set; }
    public string Asunto { get; set; } = null!;
    public string Texto { get; set; } = null!;
    public string? Estado { get; set; }
    public string? Prioridad { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaCierre { get; set; }

    public virtual ICollection<ArchivosAdjunto> ArchivosAdjuntos { get; set; } = new List<ArchivosAdjunto>();

    // CAMBIO: Haz esta propiedad nulable para evitar errores de validación en el POST
    public virtual Usuario? IdClienteNavigation { get; set; } = null!;

    public virtual Usuario? IdTecnicoNavigation { get; set; }

    public virtual ICollection<SeguimientoTicket> SeguimientoTickets { get; set; } = new List<SeguimientoTicket>();
}