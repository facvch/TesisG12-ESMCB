using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IPropietarioRepository : IRepository<Propietario>
    {
        Task<Propietario> GetByDNIAsync(string dni);
        Task<IEnumerable<Propietario>> SearchByNombreAsync(string nombre);
        Task<IEnumerable<Propietario>> GetActivosAsync();
    }
}
