using System;
using System.Collections.Generic;

namespace Tickets.Models;

public partial class ArchivosAdjunto
{
    public int IdArchivo { get; set; }

    public int IdTicket { get; set; }

    public string? NombreArchivo { get; set; }

    public string? RutaArchivo { get; set; }

    public DateTime? FechaSubida { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
