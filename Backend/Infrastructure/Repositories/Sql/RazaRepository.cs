using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class RazaRepository : BaseRepository<Raza>, IRazaRepository
    {
        public RazaRepository(StoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Raza>> GetActivasAsync()
        {
            return await Repository.Where(r => r.Activo).ToListAsync();
        }

        public async Task<IEnumerable<Raza>> GetByEspecieIdAsync(int especieId)
        {
            return await Repository.Where(r => r.EspecieId == especieId && r.Activo).ToListAsync();
        }

        public async Task<Raza> GetByNombreAsync(string nombre)
        {
            return await Repository.FirstOrDefaultAsync(r => r.Nombre == nombre);
        }
    }
}
