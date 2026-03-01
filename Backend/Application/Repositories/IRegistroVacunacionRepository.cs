using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IRegistroVacunacionRepository : IRepository<RegistroVacunacion>
    {
        Task<IEnumerable<RegistroVacunacion>> GetByPacienteIdAsync(string pacienteId);
        Task<IEnumerable<RegistroVacunacion>> GetVacunasPendientesAsync();
        Task<IEnumerable<RegistroVacunacion>> GetByVacunaIdAsync(int vacunaId);
    }
}
