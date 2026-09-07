using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Proveedore
{
    public int IdProveedor { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Contacto { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public virtual ICollection<Materiale> Materiales { get; set; } = new List<Materiale>();
}
