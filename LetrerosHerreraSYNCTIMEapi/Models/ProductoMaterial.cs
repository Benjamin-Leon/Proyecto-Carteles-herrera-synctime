using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class ProductoMaterial
{
    public int IdProducto { get; set; }

    public int IdMaterial { get; set; }

    public decimal CantidadRequerida { get; set; }

    public virtual Materiale IdMaterialNavigation { get; set; } = null!;

    public virtual ProductosServicio IdProductoNavigation { get; set; } = null!;
}
