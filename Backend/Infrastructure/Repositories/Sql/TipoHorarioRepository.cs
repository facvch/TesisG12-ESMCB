using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class TipoHorarioRepository : BaseRepository<TipoHorario>, ITipoHorarioRepository
    {
        public TipoHorarioRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<TipoHorario>> GetActivosAsync()
        {
            return await Repository.Where(t => t.Activo).ToListAsync();
        }
    }
}
