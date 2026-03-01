using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class VacunaRepository : BaseRepository<Vacuna>, IVacunaRepository
    {
        public VacunaRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Vacuna>> GetActivasAsync()
        {
            return await Repository.Where(v => v.Activo).ToListAsync();
        }

        public async Task<Vacuna> GetByNombreAsync(string nombre)
        {
            return await Repository.FirstOrDefaultAsync(v => v.Nombre == nombre);
        }
    }
}
