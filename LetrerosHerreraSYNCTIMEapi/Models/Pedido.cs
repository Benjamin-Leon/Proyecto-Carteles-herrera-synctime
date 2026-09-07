using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public int IdSolicitud { get; set; }

    public int? IdCotizacion { get; set; }

    public int IdCliente { get; set; }

    public int? IdEmpleadoAsignado { get; set; }

    public DateTime FechaPedido { get; set; }

    public string Estado { get; set; } = null!;

    public string Prioridad { get; set; } = null!;

    public virtual ICollection<FechasComprometida> FechasComprometida { get; set; } = new List<FechasComprometida>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Cotizacione? IdCotizacionNavigation { get; set; }

    public virtual Empleado? IdEmpleadoAsignadoNavigation { get; set; }

    public virtual Solicitude IdSolicitudNavigation { get; set; } = null!;

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();

    public virtual ICollection<PedidoMaterial> PedidoMaterials { get; set; } = new List<PedidoMaterial>();
}
