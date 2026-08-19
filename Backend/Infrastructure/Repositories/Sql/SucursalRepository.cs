using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class SucursalRepository : BaseRepository<Sucursal>, ISucursalRepository
    {
        public SucursalRepository(StoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Sucursal>> GetActivasAsync()
        {
            return await Repository.Where(s => s.Activa).ToListAsync();
        }
    }
}
