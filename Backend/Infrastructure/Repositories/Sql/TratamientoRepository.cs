using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class TratamientoRepository : BaseRepository<Tratamiento>, ITratamientoRepository
    {
        public TratamientoRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Tratamiento>> GetByPacienteIdAsync(string pacienteId)
        {
            return await Repository
                .Where(t => t.PacienteId == pacienteId)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tratamiento>> GetActivosAsync(string pacienteId)
        {
            return await Repository
                .Where(t => t.PacienteId == pacienteId && !t.Finalizado)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();
        }
    }
}
