using System.Security.Claims;
using LetrerosHerreraSYNCTIMEapi.Models;
using LetrerosHerreraSYNCTIMEapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LetrerosHerreraSYNCTIMEapi.Endpoints;

public static class AuthApi
{
    public static void MapAuthApi(this WebApplication app)
    {
        // Este grupo concentra login, perfil y administracion de usuarios.
        var auth = app.MapGroup("/api/v1/auth").WithTags("Autenticacion");

        // Login publico: valida credenciales y devuelve el JWT.
        auth.MapPost("/login", async (
            LoginRequest request,
            LetrerosHerreradbContext db,
            AuthService authService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["credentials"] = ["Email y password son obligatorios."]
                });

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email.Trim() && u.Activo, cancellationToken);

            if (usuario is null || !authService.VerifyPassword(request.Password, usuario.PasswordHash))
                return Results.Unauthorized();

            return Results.Ok(new
            {
                token = authService.CreateToken(usuario),
                usuario = new { usuario.IdUsuario, usuario.Nombre, usuario.Email, usuario.Rol }
            });
        }).AllowAnonymous().RequireRateLimiting("Login");

        // Solo el dueno puede crear usuarios y asignar roles.
        auth.MapPost("/usuarios", [Authorize(Roles = "dueno")] async (
            CreateUserRequest request,
            LetrerosHerreradbContext db,
            AuthService authService,
            CancellationToken cancellationToken) =>
        {
            var rolesValidos = new[] { "dueno", "disenador", "operario", "otro" };
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Nombre))
                errors["nombre"] = ["El nombre es obligatorio."];
            if (string.IsNullOrWhiteSpace(request.Email))
                errors["email"] = ["El email es obligatorio."];
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                errors["password"] = ["La password debe tener al menos 8 caracteres."];
            if (!rolesValidos.Contains(request.Rol, StringComparer.OrdinalIgnoreCase))
                errors["rol"] = ["El rol no es valido."];

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var email = request.Email.Trim().ToLowerInvariant();
            if (await db.Usuarios.AnyAsync(u => u.Email == email, cancellationToken))
                return Results.Conflict(new { message = "Ya existe un usuario con ese email." });

            if (request.IdEmpleado.HasValue && !await db.Empleados.AnyAsync(
                    e => e.IdEmpleado == request.IdEmpleado.Value,
                    cancellationToken))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["idEmpleado"] = ["El empleado indicado no existe."]
                });
            }

            var usuario = new Usuario
            {
                IdEmpleado = request.IdEmpleado,
                Nombre = request.Nombre.Trim(),
                Email = email,
                PasswordHash = authService.HashPassword(request.Password),
                Rol = rolesValidos.First(r => r.Equals(request.Rol, StringComparison.OrdinalIgnoreCase)),
                Activo = true
            };

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/auth/usuarios/{usuario.IdUsuario}", new
            {
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.Email,
                usuario.Rol,
                usuario.IdEmpleado,
                usuario.Activo
            });
        });

        auth.MapGet("/me", [Authorize] (ClaimsPrincipal principal) => Results.Ok(new
        {
            id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
            nombre = principal.Identity?.Name,
            email = principal.FindFirstValue(ClaimTypes.Email),
            rol = principal.FindFirstValue(ClaimTypes.Role)
        }));
    }

    public sealed record LoginRequest(string Email, string Password);

    public sealed record CreateUserRequest(
        string Nombre,
        string Email,
        string Password,
        string Rol,
        int? IdEmpleado = null);
}
