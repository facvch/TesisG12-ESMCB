using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IEspecieRepository : IRepository<Especie>
    {
        Task<Especie> GetByNombreAsync(string nombre);
        Task<IEnumerable<Especie>> GetActivasAsync();
    }
}
