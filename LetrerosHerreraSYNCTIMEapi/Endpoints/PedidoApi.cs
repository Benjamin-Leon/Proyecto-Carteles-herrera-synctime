using LetrerosHerreraSYNCTIMEapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LetrerosHerreraSYNCTIMEapi.Endpoints;

public static class PedidoApi
{
    public static void MapPedidoApi(this WebApplication app)
    {
        // El pedido representa la ejecucion de una solicitud y su cotizacion.
        // Es la etapa donde el trabajo deja de ser una propuesta y pasa a ejecutarse.
        var pedidos = app.MapGroup("/api/v1/pedidos")
            .WithTags("Pedidos")
            .RequireAuthorization()
            .RequireRateLimiting("ApiGeneral");

        pedidos.MapGet("/", async (LetrerosHerreradbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.Pedidos.AsNoTracking().ToListAsync(cancellationToken)));

        pedidos.MapGet("/{id:int}", async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var pedido = await db.Pedidos.AsNoTracking()
                .Include(p => p.PedidoMaterials)
                .Include(p => p.FechasComprometida)
                .FirstOrDefaultAsync(p => p.IdPedido == id, cancellationToken);

            return pedido is null
                ? Results.NotFound(new { message = "El pedido no existe." })
                : Results.Ok(pedido);
        });

        // Crear y actualizar pedidos queda para los roles que gestionan la operacion diaria.
        pedidos.MapPost("/", [Authorize(Roles = "dueno,operario")] async (
            PedidoRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var validation = await ValidateRequest(request, db, cancellationToken);
            if (validation is not null)
                return validation;

            var pedido = new Pedido
            {
                IdSolicitud = request.IdSolicitud,
                IdCotizacion = request.IdCotizacion,
                IdCliente = request.IdCliente,
                IdEmpleadoAsignado = request.IdEmpleadoAsignado,
                Estado = request.Estado,
                Prioridad = request.Prioridad
            };

            db.Pedidos.Add(pedido);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/pedidos/{pedido.IdPedido}", pedido);
        });

        pedidos.MapPut("/{id:int}", [Authorize(Roles = "dueno,operario")] async (
            int id,
            PedidoRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var pedido = await db.Pedidos.FindAsync([id], cancellationToken);
            if (pedido is null)
                return Results.NotFound(new { message = "El pedido no existe." });

            var validation = await ValidateRequest(request, db, cancellationToken, id);
            if (validation is not null)
                return validation;

            pedido.IdSolicitud = request.IdSolicitud;
            pedido.IdCotizacion = request.IdCotizacion;
            pedido.IdCliente = request.IdCliente;
            pedido.IdEmpleadoAsignado = request.IdEmpleadoAsignado;
            pedido.Estado = request.Estado;
            pedido.Prioridad = request.Prioridad;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(pedido);
        });

        pedidos.MapDelete("/{id:int}", [Authorize(Roles = "dueno")] async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var pedido = await db.Pedidos.FindAsync([id], cancellationToken);
            if (pedido is null)
                return Results.NotFound(new { message = "El pedido no existe." });

            db.Pedidos.Remove(pedido);
            await db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });
    }

    private static async Task<IResult?> ValidateRequest(
        PedidoRequest request,
        LetrerosHerreradbContext db,
        CancellationToken cancellationToken,
        int? currentId = null)
    {
        // Se comprueba que las relaciones existan y que estado/prioridad sean valores permitidos.
        var errors = new Dictionary<string, string[]>();
        var estados = new[] { "cancelado", "entregado", "terminado", "en_proceso" };
        var prioridades = new[] { "baja", "media", "alta" };

        if (!await db.Solicitudes.AnyAsync(s => s.IdSolicitud == request.IdSolicitud, cancellationToken))
            errors["idSolicitud"] = ["La solicitud indicada no existe."];
        if (!await db.Clientes.AnyAsync(c => c.IdCliente == request.IdCliente, cancellationToken))
            errors["idCliente"] = ["El cliente indicado no existe."];
        if (request.IdEmpleadoAsignado.HasValue && !await db.Empleados.AnyAsync(
                e => e.IdEmpleado == request.IdEmpleadoAsignado, cancellationToken))
            errors["idEmpleadoAsignado"] = ["El empleado indicado no existe."];
        if (request.IdCotizacion.HasValue && !await db.Cotizaciones.AnyAsync(
                c => c.IdCotizacion == request.IdCotizacion, cancellationToken))
            errors["idCotizacion"] = ["La cotizacion indicada no existe."];
        if (!estados.Contains(request.Estado))
            errors["estado"] = ["El estado del pedido no es valido."];
        if (!prioridades.Contains(request.Prioridad))
            errors["prioridad"] = ["La prioridad no es valida."];

        var solicitudRepetida = await db.Pedidos.AnyAsync(
            p => p.IdSolicitud == request.IdSolicitud && p.IdPedido != currentId,
            cancellationToken);
        if (solicitudRepetida)
            errors["idSolicitud"] = ["La solicitud ya tiene un pedido asociado."];

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    public sealed record PedidoRequest(
        int IdSolicitud,
        int? IdCotizacion,
        int IdCliente,
        int? IdEmpleadoAsignado,
        string Estado,
        string Prioridad);
}
