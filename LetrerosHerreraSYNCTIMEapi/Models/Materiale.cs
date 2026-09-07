using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Materiale
{
    public int IdMaterial { get; set; }

    public int? IdProveedor { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Categoria { get; set; }

    public string UnidadMedida { get; set; } = null!;

    public decimal StockActual { get; set; }

    public decimal StockMinimo { get; set; }

    public decimal CostoUnitario { get; set; }

    public virtual Proveedore? IdProveedorNavigation { get; set; }

    public virtual ICollection<MovimientosInventario> MovimientosInventarios { get; set; } = new List<MovimientosInventario>();

    public virtual ICollection<PedidoMaterial> PedidoMaterials { get; set; } = new List<PedidoMaterial>();

    public virtual ICollection<ProductoMaterial> ProductoMaterials { get; set; } = new List<ProductoMaterial>();
}
