using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IHistorialClinicoRepository : IRepository<HistorialClinico>
    {
        Task<IEnumerable<HistorialClinico>> GetByPacienteIdAsync(string pacienteId);
    }
}
