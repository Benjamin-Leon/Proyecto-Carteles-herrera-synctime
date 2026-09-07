using System.Text;
using LetrerosHerreraSYNCTIMEapi.Endpoints;
using LetrerosHerreraSYNCTIMEapi.Middleware;
using LetrerosHerreraSYNCTIMEapi.Models;
using LetrerosHerreraSYNCTIMEapi.OpenApi;
using LetrerosHerreraSYNCTIMEapi.Repositories;
using LetrerosHerreraSYNCTIMEapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de infraestructura: base de datos, secretos y autenticacion.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontro ConnectionStrings:DefaultConnection. Configure la variable de entorno ConnectionStrings__DefaultConnection.");

builder.Services.AddDbContext<LetrerosHerreradbContext>(options =>
    options.UseSqlServer(connectionString));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("No se configuro Jwt:Key.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("No se configuro Jwt:Issuer.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("No se configuro Jwt:Audience.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();
builder.Services.AddProblemDetails();

// Controles transversales: solo el frontend permitido y limites contra abuso.
builder.Services.AddCors(options =>
{
    var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "https://localhost:3000";
    options.AddPolicy("FrontendPermitido", policy =>
        policy.WithOrigins(allowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("ApiGeneral", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 100;
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("Login", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 10;
        limiter.QueueLimit = 0;
    });
});

// Documentacion automatica y esquema Bearer para probar JWT desde Scalar.
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<RequestLoggingMiddleware>();
// Estos controles se aplican antes de ejecutar cualquier endpoint.
app.UseCors("FrontendPermitido");
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
// Primero se identifica al usuario; despues se revisan sus roles y permisos.
app.UseAuthentication();
app.UseAuthorization();

// Registro de rutas por area del negocio, al estilo de GamesApi.
app.MapAuthApi();
app.MapCatalogoApi();
app.MapSolicitudApi();
app.MapCotizacionApi();
app.MapPedidoApi();

app.Run();

public partial class Program;
