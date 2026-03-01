using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class ServicioRepository : BaseRepository<Servicio>, IServicioRepository
    {
        public ServicioRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Servicio>> GetActivosAsync()
        {
            return await Repository.Where(s => s.Activo).ToListAsync();
        }

        public async Task<Servicio> GetByNombreAsync(string nombre)
        {
            return await Repository.FirstOrDefaultAsync(s => s.Nombre == nombre);
        }
    }
}
