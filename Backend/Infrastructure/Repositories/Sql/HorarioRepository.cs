using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class HorarioRepository : BaseRepository<Horario>, IHorarioRepository
    {
        public HorarioRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Horario>> GetActivosAsync()
        {
            return await Repository
                .Include(h => h.TipoHorario)
                .Where(h => h.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Horario>> GetByVeterinarioIdAsync(string veterinarioId)
        {
            return await Repository
                .Include(h => h.TipoHorario)
                .Where(h => h.VeterinarioId == veterinarioId && h.Activo)
                .ToListAsync();
        }

        public async Task DeleteByVeterinarioIdAsync(string veterinarioId)
        {
            var horarios = await Repository.Where(h => h.VeterinarioId == veterinarioId).ToListAsync();
            if (horarios.Any())
            {
                Context.RemoveRange(horarios);
                await Context.SaveChangesAsync();
            }
        }
    }
}
