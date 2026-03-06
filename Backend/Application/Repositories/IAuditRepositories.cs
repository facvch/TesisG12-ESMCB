using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByFechaRangoAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<AuditLog>> GetByUsuarioIdAsync(string usuarioId);
        Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad);
        Task<IEnumerable<AuditLog>> GetUltimosAsync(int cantidad);
    }

    public interface INotificacionRepository : IRepository<Notificacion>
    {
        Task<IEnumerable<Notificacion>> GetNoLeidasAsync();
        Task<IEnumerable<Notificacion>> GetByTipoAsync(TipoNotificacion tipo);
        Task<IEnumerable<Notificacion>> GetUltimasAsync(int cantidad);
    }
}
