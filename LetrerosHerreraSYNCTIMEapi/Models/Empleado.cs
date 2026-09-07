using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Empleado
{
    public int IdEmpleado { get; set; }

    public string Nombre { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();

    public virtual ICollection<HorariosAtencion> HorariosAtencions { get; set; } = new List<HorariosAtencion>();

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
