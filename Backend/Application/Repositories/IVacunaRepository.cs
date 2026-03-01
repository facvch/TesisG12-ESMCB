using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IVacunaRepository : IRepository<Vacuna>
    {
        Task<Vacuna> GetByNombreAsync(string nombre);
        Task<IEnumerable<Vacuna>> GetActivasAsync();
    }
}
