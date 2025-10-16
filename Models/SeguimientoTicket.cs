using System;
using System.Collections.Generic;

namespace Tickets.Models;

public partial class SeguimientoTicket
{
    public int IdSeguimiento { get; set; }

    public int IdTicket { get; set; }

    public int IdUsuario { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateTime? Fecha { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
