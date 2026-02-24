using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IRazaRepository : IRepository<Raza>
    {
        Task<IEnumerable<Raza>> GetByEspecieIdAsync(int especieId);
        Task<Raza> GetByNombreAsync(string nombre);
        Task<IEnumerable<Raza>> GetActivasAsync();
    }
}
