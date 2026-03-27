using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IPacienteRepository : IRepository<Paciente>
    {
        Task<IEnumerable<Paciente>> GetByPropietarioIdAsync(string propietarioId);
        Task<IEnumerable<Paciente>> GetByEspecieIdAsync(int especieId);
        Task<IEnumerable<Paciente>> SearchByNombreAsync(string nombre);
        Task<IEnumerable<Paciente>> GetActivosAsync();
        Task<IEnumerable<Paciente>> GetPacientesExpandidosAsync();
    }
}
