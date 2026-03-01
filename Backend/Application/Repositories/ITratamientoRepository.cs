using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface ITratamientoRepository : IRepository<Tratamiento>
    {
        Task<IEnumerable<Tratamiento>> GetByPacienteIdAsync(string pacienteId);
        Task<IEnumerable<Tratamiento>> GetActivosAsync(string pacienteId);
    }
}
