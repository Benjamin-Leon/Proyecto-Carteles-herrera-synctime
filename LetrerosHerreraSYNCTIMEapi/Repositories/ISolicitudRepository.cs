using LetrerosHerreraSYNCTIMEapi.Models;

namespace LetrerosHerreraSYNCTIMEapi.Repositories;

public interface ISolicitudRepository
{
    // La interfaz define lo que el endpoint necesita, sin exponer como se consulta SQL Server.
    Task<IReadOnlyList<Solicitude>> GetAllAsync(CancellationToken cancellationToken);
    Task<Solicitude?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ClienteExistsAsync(int idCliente, CancellationToken cancellationToken);
    Task<bool> ProductoExistsAsync(int idProducto, CancellationToken cancellationToken);
    Task AddAsync(Solicitude solicitud, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    void Remove(Solicitude solicitud);
}
