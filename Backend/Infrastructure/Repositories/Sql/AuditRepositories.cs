using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByFechaRangoAsync(DateTime desde, DateTime hasta) =>
            await Repository.Where(a => a.Fecha >= desde && a.Fecha <= hasta)
                .OrderByDescending(a => a.Fecha).ToListAsync();

        public async Task<IEnumerable<AuditLog>> GetByUsuarioIdAsync(string usuarioId) =>
            await Repository.Where(a => a.UsuarioId == usuarioId)
                .OrderByDescending(a => a.Fecha).Take(100).ToListAsync();

        public async Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad) =>
            await Repository.Where(a => a.Entidad == entidad)
                .OrderByDescending(a => a.Fecha).Take(100).ToListAsync();

        public async Task<IEnumerable<AuditLog>> GetUltimosAsync(int cantidad) =>
            await Repository.OrderByDescending(a => a.Fecha).Take(cantidad).ToListAsync();
    }

    internal class NotificacionRepository : BaseRepository<Notificacion>, INotificacionRepository
    {
        public NotificacionRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Notificacion>> GetNoLeidasAsync() =>
            await Repository.Where(n => !n.Leida).OrderByDescending(n => n.FechaCreacion).ToListAsync();

        public async Task<IEnumerable<Notificacion>> GetByTipoAsync(TipoNotificacion tipo) =>
            await Repository.Where(n => n.Tipo == tipo)
                .OrderByDescending(n => n.FechaCreacion).Take(50).ToListAsync();

        public async Task<IEnumerable<Notificacion>> GetUltimasAsync(int cantidad) =>
            await Repository.OrderByDescending(n => n.FechaCreacion).Take(cantidad).ToListAsync();
    }
}
