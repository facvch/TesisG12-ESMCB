using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IServicioRepository : IRepository<Servicio>
    {
        Task<Servicio> GetByNombreAsync(string nombre);
        Task<IEnumerable<Servicio>> GetActivosAsync();
    }
}
