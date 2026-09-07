using LetrerosHerreraSYNCTIMEapi.Models;
using Microsoft.EntityFrameworkCore;

namespace LetrerosHerreraSYNCTIMEapi.Repositories;

public sealed class SolicitudRepository(LetrerosHerreradbContext db) : ISolicitudRepository
{
    // El repositorio concentra consultas y persistencia de solicitudes para no acoplar
    // ese flujo directamente a los detalles del DbContext en todos los endpoints.
    public async Task<IReadOnlyList<Solicitude>> GetAllAsync(
        CancellationToken cancellationToken) =>
        await db.Solicitudes
            .AsNoTracking()
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync(cancellationToken);

    public Task<Solicitude?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Solicitudes
            .FirstOrDefaultAsync(s => s.IdSolicitud == id, cancellationToken);

    public Task<bool> ClienteExistsAsync(int idCliente, CancellationToken cancellationToken) =>
        db.Clientes.AnyAsync(c => c.IdCliente == idCliente, cancellationToken);

    public Task<bool> ProductoExistsAsync(int idProducto, CancellationToken cancellationToken) =>
        db.ProductosServicios.AnyAsync(p => p.IdProducto == idProducto, cancellationToken);

    public async Task AddAsync(Solicitude solicitud, CancellationToken cancellationToken) =>
        await db.Solicitudes.AddAsync(solicitud, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    public void Remove(Solicitude solicitud) => db.Solicitudes.Remove(solicitud);
}
