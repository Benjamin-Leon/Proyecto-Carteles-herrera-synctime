using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class PedidoMaterial
{
    public int IdPedido { get; set; }

    public int IdMaterial { get; set; }

    public decimal CantidadUtilizada { get; set; }

    public virtual Materiale IdMaterialNavigation { get; set; } = null!;

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;
}
