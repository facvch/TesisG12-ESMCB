using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class HistorialClinicoRepository : BaseRepository<HistorialClinico>, IHistorialClinicoRepository
    {
        public HistorialClinicoRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<HistorialClinico>> GetByPacienteIdAsync(string pacienteId)
        {
            return await Repository
                .Where(h => h.PacienteId == pacienteId)
                .OrderByDescending(h => h.Fecha)
                .ToListAsync();
        }
    }
}
