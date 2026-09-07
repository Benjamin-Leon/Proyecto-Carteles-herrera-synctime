using LetrerosHerreraSYNCTIMEapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LetrerosHerreraSYNCTIMEapi.Endpoints;

public static class CotizacionApi
{
    public static void MapCotizacionApi(this WebApplication app)
    {
        // Una cotizacion contiene detalles y calcula su total antes de persistir.
        // En palabras simples: transforma una solicitud en una propuesta con precio.
        var cotizaciones = app.MapGroup("/api/v1/cotizaciones")
            .WithTags("Cotizaciones")
            .RequireAuthorization()
            .RequireRateLimiting("ApiGeneral");

        // GET devuelve cotizaciones junto con sus lineas de detalle.
        cotizaciones.MapGet("/", async (LetrerosHerreradbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.Cotizaciones
                .AsNoTracking()
                .Include(c => c.CotizacionDetalles)
                .ToListAsync(cancellationToken)));

        cotizaciones.MapGet("/{id:int}", async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var cotizacion = await db.Cotizaciones
                .AsNoTracking()
                .Include(c => c.CotizacionDetalles)
                .FirstOrDefaultAsync(c => c.IdCotizacion == id, cancellationToken);

            return cotizacion is null
                ? Results.NotFound(new { message = "La cotizacion no existe." })
                : Results.Ok(cotizacion);
        });

        // POST crea la cabecera y todos sus detalles en una sola operacion.
        cotizaciones.MapPost("/", [Authorize(Roles = "dueno,disenador")] async (
            CotizacionRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var validation = await ValidateRequest(request, db, cancellationToken);
            if (validation is not null)
                return validation;

            var cotizacion = new Cotizacione
            {
                IdSolicitud = request.IdSolicitud,
                IdCliente = request.IdCliente,
                IdEmpleado = request.IdEmpleado,
                Estado = "pendiente",
                ValidezDias = request.ValidezDias,
                Total = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                CotizacionDetalles = request.Detalles.Select(d => new CotizacionDetalle
                {
                    IdProducto = d.IdProducto,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            };

            db.Cotizaciones.Add(cotizacion);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/cotizaciones/{cotizacion.IdCotizacion}", cotizacion);
        });

        cotizaciones.MapPut("/{id:int}", [Authorize(Roles = "dueno,disenador")] async (
            int id,
            CotizacionRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var cotizacion = await db.Cotizaciones
                .Include(c => c.CotizacionDetalles)
                .FirstOrDefaultAsync(c => c.IdCotizacion == id, cancellationToken);

            if (cotizacion is null)
                return Results.NotFound(new { message = "La cotizacion no existe." });

            var validation = await ValidateRequest(request, db, cancellationToken);
            if (validation is not null)
                return validation;

            cotizacion.IdSolicitud = request.IdSolicitud;
            cotizacion.IdCliente = request.IdCliente;
            cotizacion.IdEmpleado = request.IdEmpleado;
            cotizacion.ValidezDias = request.ValidezDias;
            // El total se calcula en el servidor para no confiar en el precio enviado por el cliente.
            cotizacion.Total = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            db.CotizacionDetalles.RemoveRange(cotizacion.CotizacionDetalles);
            cotizacion.CotizacionDetalles = request.Detalles.Select(d => new CotizacionDetalle
            {
                IdCotizacion = cotizacion.IdCotizacion,
                IdProducto = d.IdProducto,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList();

            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(cotizacion);
        });

        cotizaciones.MapDelete("/{id:int}", [Authorize(Roles = "dueno")] async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var cotizacion = await db.Cotizaciones.FindAsync([id], cancellationToken);
            if (cotizacion is null)
                return Results.NotFound(new { message = "La cotizacion no existe." });

            db.Cotizaciones.Remove(cotizacion);
            await db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });
    }

    private static async Task<IResult?> ValidateRequest(
        CotizacionRequest request,
        LetrerosHerreradbContext db,
        CancellationToken cancellationToken)
    {
        // Antes de guardar se revisan relaciones, cantidades, precios y fechas de validez.
        var errors = new Dictionary<string, string[]>();

        if (request.IdSolicitud.HasValue && !await db.Solicitudes.AnyAsync(
                s => s.IdSolicitud == request.IdSolicitud.Value, cancellationToken))
            errors["idSolicitud"] = ["La solicitud indicada no existe."];
        if (request.IdCliente <= 0 || !await db.Clientes.AnyAsync(c => c.IdCliente == request.IdCliente, cancellationToken))
            errors["idCliente"] = ["El cliente indicado no existe."];
        if (request.IdEmpleado.HasValue && !await db.Empleados.AnyAsync(e => e.IdEmpleado == request.IdEmpleado, cancellationToken))
            errors["idEmpleado"] = ["El empleado indicado no existe."];
        if (request.ValidezDias <= 0)
            errors["validezDias"] = ["La validez debe ser mayor que cero."];
        if (request.Detalles.Count == 0)
            errors["detalles"] = ["Debe incluir al menos un detalle."];
        if (request.Detalles.Any(d => d.Cantidad <= 0 || d.PrecioUnitario < 0))
            errors["detalles"] = ["Las cantidades deben ser mayores que cero y los precios no pueden ser negativos."];

        var productos = request.Detalles.Select(d => d.IdProducto).Distinct().ToList();
        if (productos.Count > 0 && await db.ProductosServicios.CountAsync(
                p => productos.Contains(p.IdProducto), cancellationToken) != productos.Count)
        {
            errors["detalles"] = ["Uno o mas productos no existen."];
        }

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    public sealed record CotizacionRequest(
        int? IdSolicitud,
        int IdCliente,
        int? IdEmpleado,
        int ValidezDias,
        List<CotizacionDetalleRequest> Detalles);

    public sealed record CotizacionDetalleRequest(
        int IdProducto,
        string? Descripcion,
        decimal Cantidad,
        decimal PrecioUnitario);
}