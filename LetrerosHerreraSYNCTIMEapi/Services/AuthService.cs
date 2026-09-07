using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LetrerosHerreraSYNCTIMEapi.Models;
using Microsoft.IdentityModel.Tokens;

namespace LetrerosHerreraSYNCTIMEapi.Services;

public sealed class AuthService(IConfiguration configuration)
{
    // BCrypt almacena contrasenas como hashes no reversibles.
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);

    public string CreateToken(Usuario usuario)
    {
        // El token transporta la identidad y el rol que usara Authorization.
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("No se configuro Jwt:Key.");
        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("No se configuro Jwt:Issuer.");
        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("No se configuro Jwt:Audience.");
        var expirationMinutes = configuration.GetValue("Jwt:ExpirationMinutes", 15);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(expirationMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
