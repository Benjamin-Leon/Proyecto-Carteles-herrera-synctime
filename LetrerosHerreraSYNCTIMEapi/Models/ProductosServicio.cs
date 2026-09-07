using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class ProductosServicio
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? UnidadMedida { get; set; }

    public decimal PrecioBase { get; set; }

    public virtual ICollection<CotizacionDetalle> CotizacionDetalles { get; set; } = new List<CotizacionDetalle>();

    public virtual ICollection<ProductoMaterial> ProductoMaterials { get; set; } = new List<ProductoMaterial>();

    public virtual ICollection<Solicitude> Solicitudes { get; set; } = new List<Solicitude>();
}
