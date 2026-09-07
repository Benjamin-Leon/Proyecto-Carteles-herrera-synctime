using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Solicitude
{
    public int IdSolicitud { get; set; }

    public int IdCliente { get; set; }

    public int? IdProducto { get; set; }

    public string? Tamano { get; set; }

    public string? MaterialSolicitado { get; set; }

    public int Cantidad { get; set; }

    public string TelefonoContacto { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime? FechaDecision { get; set; }

    public string? NotasWhatsapp { get; set; }

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual ProductosServicio? IdProductoNavigation { get; set; }

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual Pedido? Pedido { get; set; }
}
