using LetrerosHerreraSYNCTIMEapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LetrerosHerreraSYNCTIMEapi.Endpoints;

public static class CatalogoApi
{
    public static void MapCatalogoApi(this WebApplication app)
    {
        // Catalogo y consultas operativas: recursos base que alimentan el flujo comercial.
        MapClientes(app);
        MapProductos(app);
        MapConsultasOperativas(app);
    }

    private static void MapClientes(WebApplication app)
    {
        // Clientes requieren sesion; las operaciones de escritura tienen roles mas estrictos.
        var clientes = app.MapGroup("/api/v1/clientes")
            .WithTags("Clientes")
            .RequireAuthorization()
            .RequireRateLimiting("ApiGeneral");

        clientes.MapGet("/", async (LetrerosHerreradbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.Clientes.AsNoTracking().ToListAsync(cancellationToken)));

        clientes.MapGet("/{id:int}", async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var cliente = await db.Clientes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCliente == id, cancellationToken);
            return cliente is null
                ? Results.NotFound(new { message = "El cliente no existe." })
                : Results.Ok(cliente);
        });

        clientes.MapPost("/", [Authorize(Roles = "dueno,operario")] async (
            ClienteRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var validation = ValidateCliente(request);
            if (validation is not null)
                return validation;

            if (await db.Clientes.AnyAsync(c => c.Telefono == request.Telefono.Trim(), cancellationToken))
                return Results.Conflict(new { message = "Ya existe un cliente con ese telefono." });

            var cliente = new Cliente
            {
                Nombre = request.Nombre?.Trim(),
                Telefono = request.Telefono.Trim(),
                Email = request.Email?.Trim().ToLowerInvariant(),
                Direccion = request.Direccion?.Trim(),
                CanalPreferido = request.CanalPreferido ?? "web"
            };

            db.Clientes.Add(cliente);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/clientes/{cliente.IdCliente}", cliente);
        });

        clientes.MapPut("/{id:int}", [Authorize(Roles = "dueno,operario")] async (
            int id,
            ClienteRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var validation = ValidateCliente(request);
            if (validation is not null)
                return validation;

            var cliente = await db.Clientes.FindAsync([id], cancellationToken);
            if (cliente is null)
                return Results.NotFound(new { message = "El cliente no existe." });

            var telefonoEnUso = await db.Clientes.AnyAsync(
                c => c.IdCliente != id && c.Telefono == request.Telefono.Trim(),
                cancellationToken);
            if (telefonoEnUso)
                return Results.Conflict(new { message = "El telefono ya pertenece a otro cliente." });

            cliente.Nombre = request.Nombre?.Trim();
            cliente.Telefono = request.Telefono.Trim();
            cliente.Email = request.Email?.Trim().ToLowerInvariant();
            cliente.Direccion = request.Direccion?.Trim();
            cliente.CanalPreferido = request.CanalPreferido ?? cliente.CanalPreferido;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(cliente);
        });

        clientes.MapDelete("/{id:int}", [Authorize(Roles = "dueno")] async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var cliente = await db.Clientes.FindAsync([id], cancellationToken);
            if (cliente is null)
                return Results.NotFound(new { message = "El cliente no existe." });

            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });
    }

    private static void MapProductos(WebApplication app)
    {
        // Productos son lectura publica, pero solo el dueno puede modificarlos.
        var productos = app.MapGroup("/api/v1/productos")
            .WithTags("Productos")
            .RequireRateLimiting("ApiGeneral");

        productos.MapGet("/", async (LetrerosHerreradbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.ProductosServicios.AsNoTracking().ToListAsync(cancellationToken)))
            .AllowAnonymous();

        productos.MapGet("/{id:int}", async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var producto = await db.ProductosServicios.AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProducto == id, cancellationToken);
            return producto is null
                ? Results.NotFound(new { message = "El producto no existe." })
                : Results.Ok(producto);
        }).AllowAnonymous();

        productos.MapPost("/", [Authorize(Roles = "dueno")] async (
            ProductoRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var validation = ValidateProducto(request);
            if (validation is not null)
                return validation;

            var producto = new ProductosServicio
            {
                Nombre = request.Nombre.Trim(),
                Tipo = request.Tipo.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                UnidadMedida = request.UnidadMedida?.Trim(),
                PrecioBase = request.PrecioBase
            };

            db.ProductosServicios.Add(producto);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/productos/{producto.IdProducto}", producto);
        });

        productos.MapPut("/{id:int}", [Authorize(Roles = "dueno")] async (
            int id,
            ProductoRequest request,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var validation = ValidateProducto(request);
            if (validation is not null)
                return validation;

            var producto = await db.ProductosServicios.FindAsync([id], cancellationToken);
            if (producto is null)
                return Results.NotFound(new { message = "El producto no existe." });

            producto.Nombre = request.Nombre.Trim();
            producto.Tipo = request.Tipo.Trim();
            producto.Descripcion = request.Descripcion?.Trim();
            producto.UnidadMedida = request.UnidadMedida?.Trim();
            producto.PrecioBase = request.PrecioBase;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(producto);
        });

        productos.MapDelete("/{id:int}", [Authorize(Roles = "dueno")] async (
            int id,
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
        {
            var producto = await db.ProductosServicios.FindAsync([id], cancellationToken);
            if (producto is null)
                return Results.NotFound(new { message = "El producto no existe." });

            db.ProductosServicios.Remove(producto);
            await db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });
    }

    private static void MapConsultasOperativas(WebApplication app)
    {
        app.MapGet("/api/v1/empleados", [Authorize] async (
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
            Results.Ok(await db.Empleados.AsNoTracking().ToListAsync(cancellationToken)))
            .WithTags("Empleados")
            .RequireRateLimiting("ApiGeneral");

        app.MapGet("/api/v1/materiales", [Authorize] async (
            LetrerosHerreradbContext db,
            CancellationToken cancellationToken) =>
            Results.Ok(await db.Materiales.AsNoTracking().ToListAsync(cancellationToken)))
            .WithTags("Materiales")
            .RequireRateLimiting("ApiGeneral");

    }

    private static IResult? ValidateCliente(ClienteRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Telefono))
            errors["telefono"] = ["El telefono es obligatorio."];
        if (request.CanalPreferido is not null &&
            !new[] { "otro", "presencial", "web", "whatsapp" }.Contains(request.CanalPreferido))
            errors["canalPreferido"] = ["El canal preferido no es valido."];

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    private static IResult? ValidateProducto(ProductoRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Nombre))
            errors["nombre"] = ["El nombre es obligatorio."];
        if (string.IsNullOrWhiteSpace(request.Tipo))
            errors["tipo"] = ["El tipo es obligatorio."];
        if (request.PrecioBase < 0)
            errors["precioBase"] = ["El precio no puede ser negativo."];

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    public sealed record ClienteRequest(
        string? Nombre,
        string Telefono,
        string? Email,
        string? Direccion,
        string? CanalPreferido);

    public sealed record ProductoRequest(
        string Nombre,
        string Tipo,
        string? Descripcion,
        string? UnidadMedida,
        decimal PrecioBase);
}