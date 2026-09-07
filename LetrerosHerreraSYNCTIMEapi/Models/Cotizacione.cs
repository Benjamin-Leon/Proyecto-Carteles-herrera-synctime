using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Cotizacione
{
    public int IdCotizacion { get; set; }

    public int? IdSolicitud { get; set; }

    public int IdCliente { get; set; }

    public int? IdEmpleado { get; set; }

    public DateTime FechaEmision { get; set; }

    public string Estado { get; set; } = null!;

    public int ValidezDias { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<CotizacionDetalle> CotizacionDetalles { get; set; } = new List<CotizacionDetalle>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado? IdEmpleadoNavigation { get; set; }

    public virtual Solicitude? IdSolicitudNavigation { get; set; }

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
