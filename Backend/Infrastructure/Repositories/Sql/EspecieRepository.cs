using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class EspecieRepository : BaseRepository<Especie>, IEspecieRepository
    {
        public EspecieRepository(StoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Especie>> GetActivasAsync()
        {
            return await Repository.Where(e => e.Activo).ToListAsync();
        }

        public async Task<Especie> GetByNombreAsync(string nombre)
        {
            return await Repository.FirstOrDefaultAsync(e => e.Nombre == nombre);
        }
    }
}
