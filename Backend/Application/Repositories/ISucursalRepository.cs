using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface ISucursalRepository : IRepository<Sucursal>
    {
        Task<IEnumerable<Sucursal>> GetActivasAsync();
    }
}
