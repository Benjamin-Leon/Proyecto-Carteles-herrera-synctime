using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int? IdEmpleado { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }
}
