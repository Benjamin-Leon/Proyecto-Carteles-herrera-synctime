using System;
using System.Collections.Generic;

namespace LetrerosHerreraSYNCTIMEapi.Models;

public partial class Notificacione
{
    public int IdNotificacion { get; set; }

    public int IdSolicitud { get; set; }

    public string Canal { get; set; } = null!;

    public string Destinatario { get; set; } = null!;

    public DateTime FechaEnvio { get; set; }

    public string EstadoEnvio { get; set; } = null!;

    public virtual Solicitude IdSolicitudNavigation { get; set; } = null!;
}
