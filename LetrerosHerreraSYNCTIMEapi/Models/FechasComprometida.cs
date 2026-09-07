using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class FechasComprometida
{
    public int IdFecha { get; set; }

    public int IdPedido { get; set; }

    public DateOnly FechaEntrega { get; set; }

    public TimeOnly? HoraEstimada { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;
}
