using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class TurnoRepository : BaseRepository<Turno>, ITurnoRepository
    {
        public TurnoRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Turno>> GetByVeterinarioIdAsync(string veterinarioId, DateTime? desde = null, DateTime? hasta = null)
        {
            var query = Repository.Where(t => t.VeterinarioId == veterinarioId);

            if (desde.HasValue)
                query = query.Where(t => t.FechaHora >= desde.Value);
            if (hasta.HasValue)
                query = query.Where(t => t.FechaHora <= hasta.Value);

            return await query.OrderBy(t => t.FechaHora).ToListAsync();
        }

        public async Task<IEnumerable<Turno>> GetByPacienteIdAsync(string pacienteId)
        {
            return await Repository
                .Where(t => t.PacienteId == pacienteId)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turno>> GetByFechaAsync(DateTime fecha)
        {
            var inicio = fecha.Date;
            var fin = inicio.AddDays(1);
            return await Repository
                .Where(t => t.FechaHora >= inicio && t.FechaHora < fin)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turno>> GetProgramadosAsync(DateTime desde, DateTime hasta)
        {
            return await Repository
                .Where(t => t.FechaHora >= desde && t.FechaHora <= hasta
                    && (t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado || t.Estado == EstadoTurno.Reprogramado))
                .OrderBy(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turno>> GetByEstadoAsync(EstadoTurno estado)
        {
            return await Repository
                .Where(t => t.Estado == estado)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();
        }
    }
}
