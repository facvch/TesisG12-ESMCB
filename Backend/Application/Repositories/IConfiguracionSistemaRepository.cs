using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IConfiguracionSistemaRepository : IRepository<ConfiguracionSistema>
    {
        Task<ConfiguracionSistema> GetByClaveAsync(string clave);
        Task<IEnumerable<ConfiguracionSistema>> GetByGrupoAsync(string grupo);
    }
}
