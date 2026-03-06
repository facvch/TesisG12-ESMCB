using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class ConfiguracionSistemaRepository : BaseRepository<ConfiguracionSistema>, IConfiguracionSistemaRepository
    {
        public ConfiguracionSistemaRepository(StoreDbContext context) : base(context) { }

        public async Task<ConfiguracionSistema> GetByClaveAsync(string clave) =>
            await Repository.FirstOrDefaultAsync(c => c.Clave == clave);

        public async Task<IEnumerable<ConfiguracionSistema>> GetByGrupoAsync(string grupo) =>
            await Repository.Where(c => c.Grupo == grupo).OrderBy(c => c.Clave).ToListAsync();
    }
}
