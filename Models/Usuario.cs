using System;
using System.Collections.Generic;

namespace Tickets.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Clave { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<SeguimientoTicket> SeguimientoTickets { get; set; } = new List<SeguimientoTicket>();

    public virtual ICollection<Ticket> TicketIdClienteNavigations { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketIdTecnicoNavigations { get; set; } = new List<Ticket>();
}
