using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class HorariosAtencion
{
    public int IdHorario { get; set; }

    public int? IdEmpleado { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public string Estado { get; set; } = null!;

    public string? Nota { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }
}
