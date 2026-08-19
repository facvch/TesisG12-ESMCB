using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface ITipoHorarioRepository : IRepository<TipoHorario>
    {
        Task<IEnumerable<TipoHorario>> GetActivosAsync();
    }
}
