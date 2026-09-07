using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string? Nombre { get; set; }

    public string Telefono { get; set; } = null!;

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public string CanalPreferido { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<Cotizacione> Cotizaciones { get; set; } = new List<Cotizacione>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual ICollection<Solicitude> Solicitudes { get; set; } = new List<Solicitude>();
}
