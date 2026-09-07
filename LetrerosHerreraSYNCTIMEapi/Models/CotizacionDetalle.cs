using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class CotizacionDetalle
{
    public int IdDetalle { get; set; }

    public int IdCotizacion { get; set; }

    public int IdProducto { get; set; }

    public string? Descripcion { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual Cotizacione IdCotizacionNavigation { get; set; } = null!;

    public virtual ProductosServicio IdProductoNavigation { get; set; } = null!;
}
