using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class MovimientosInventario
{
    public int IdMovimiento { get; set; }

    public int IdMaterial { get; set; }

    public int? IdEmpleado { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public DateTime Fecha { get; set; }

    public int? ReferenciaPedido { get; set; }

    public string? Motivo { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }

    public virtual Materiale IdMaterialNavigation { get; set; } = null!;

    public virtual Pedido? ReferenciaPedidoNavigation { get; set; }
}
