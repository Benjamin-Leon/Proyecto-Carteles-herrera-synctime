using LetrerosHerreraSYNCTIMEapi.Models;
using LetrerosHerreraSYNCTIMEapi.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace LetrerosHerreraSYNCTIMEapi.Endpoints;

public static class SolicitudApi
{
    public static void MapSolicitudApi(this WebApplication app)
    {
        // Las solicitudes inician el flujo: relacionan un cliente con un producto o servicio.
        var solicitudes = app.MapGroup("/api/v1/solicitudes")
            .WithTags("Solicitudes")
            .RequireAuthorization()
            .RequireRateLimiting("ApiGeneral");

        solicitudes.MapGet("/", async (
            ISolicitudRepository repository,
            CancellationToken cancellationToken) =>
        {
            var items = await repository.GetAllAsync(cancellationToken);
            return Results.Ok(items.Select(ToResponse));
        });

        solicitudes.MapGet("/{id:int}", async (
            int id,
            ISolicitudRepository repository,
            CancellationToken cancellationToken) =>
        {
            var solicitud = await repository.GetByIdAsync(id, cancellationToken);
            return solicitud is null
                ? Results.NotFound(new { message = "La solicitud no existe." })
                : Results.Ok(ToResponse(solicitud));
        });

        solicitudes.MapPost("/", [Authorize(Roles = "dueno,disenador,operario")] async (
            SolicitudRequest request,
            ISolicitudRepository repository,
            CancellationToken cancellationToken) =>
        {
            var validation = await ValidateRequest(request, repository, cancellationToken);
            if (validation is not null)
                return validation;

            var solicitud = new Solicitude
            {
                IdCliente = request.IdCliente,
                IdProducto = request.IdProducto,
                Tamano = request.Tamano?.Trim(),
                MaterialSolicitado = request.MaterialSolicitado?.Trim(),
                Cantidad = request.Cantidad,
                TelefonoContacto = request.TelefonoContacto.Trim(),
                Estado = "pendiente",
                NotasWhatsapp = request.NotasWhatsapp?.Trim()
            };

            await repository.AddAsync(solicitud, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/solicitudes/{solicitud.IdSolicitud}", ToResponse(solicitud));
        });

        solicitudes.MapPut("/{id:int}", [Authorize(Roles = "dueno,disenador,operario")] async (
            int id,
            SolicitudRequest request,
            ISolicitudRepository repository,
            CancellationToken cancellationToken) =>
        {
            var solicitud = await repository.GetByIdAsync(id, cancellationToken);
            if (solicitud is null)
                return Results.NotFound(new { message = "La solicitud no existe." });

            var validation = await ValidateRequest(request, repository, cancellationToken);
            if (validation is not null)
                return validation;

            solicitud.IdCliente = request.IdCliente;
            solicitud.IdProducto = request.IdProducto;
            solicitud.Tamano = request.Tamano?.Trim();
            solicitud.MaterialSolicitado = request.MaterialSolicitado?.Trim();
            solicitud.Cantidad = request.Cantidad;
            solicitud.TelefonoContacto = request.TelefonoContacto.Trim();
            solicitud.NotasWhatsapp = request.NotasWhatsapp?.Trim();

            await repository.SaveChangesAsync(cancellationToken);
            return Results.Ok(ToResponse(solicitud));
        });

        solicitudes.MapDelete("/{id:int}", [Authorize(Roles = "dueno")] async (
            int id,
            ISolicitudRepository repository,
            CancellationToken cancellationToken) =>
        {
            var solicitud = await repository.GetByIdAsync(id, cancellationToken);
            if (solicitud is null)
                return Results.NotFound(new { message = "La solicitud no existe." });

            repository.Remove(solicitud);
            await repository.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });
    }

    private static async Task<IResult?> ValidateRequest(
        SolicitudRequest request,
        ISolicitudRepository repository,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.IdCliente <= 0 || !await repository.ClienteExistsAsync(request.IdCliente, cancellationToken))
            errors["idCliente"] = ["El cliente indicado no existe."];

        if (request.IdProducto is <= 0 || request.IdProducto.HasValue &&
            !await repository.ProductoExistsAsync(request.IdProducto.Value, cancellationToken))
            errors["idProducto"] = ["El producto indicado no existe."];

        if (request.Cantidad <= 0)
            errors["cantidad"] = ["La cantidad debe ser mayor que cero."];

        if (string.IsNullOrWhiteSpace(request.TelefonoContacto))
            errors["telefonoContacto"] = ["El telefono de contacto es obligatorio."];

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    private static SolicitudResponse ToResponse(Solicitude solicitud) => new(
        solicitud.IdSolicitud,
        solicitud.IdCliente,
        solicitud.IdProducto,
        solicitud.Tamano,
        solicitud.MaterialSolicitado,
        solicitud.Cantidad,
        solicitud.TelefonoContacto,
        solicitud.FechaSolicitud,
        solicitud.Estado,
        solicitud.FechaDecision,
        solicitud.NotasWhatsapp);

    public sealed record SolicitudRequest(
        int IdCliente,
        int? IdProducto,
        string? Tamano,
        string? MaterialSolicitado,
        int Cantidad,
        string TelefonoContacto,
        string? NotasWhatsapp);

    public sealed record SolicitudResponse(
        int IdSolicitud,
        int IdCliente,
        int? IdProducto,
        string? Tamano,
        string? MaterialSolicitado,
        int Cantidad,
        string TelefonoContacto,
        DateTime FechaSolicitud,
        string Estado,
        DateTime? FechaDecision,
        string? NotasWhatsapp);
}
