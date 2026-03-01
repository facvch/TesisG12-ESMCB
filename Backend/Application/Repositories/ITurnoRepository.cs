using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface ITurnoRepository : IRepository<Turno>
    {
        Task<IEnumerable<Turno>> GetByVeterinarioIdAsync(string veterinarioId, DateTime? desde = null, DateTime? hasta = null);
        Task<IEnumerable<Turno>> GetByPacienteIdAsync(string pacienteId);
        Task<IEnumerable<Turno>> GetByFechaAsync(DateTime fecha);
        Task<IEnumerable<Turno>> GetProgramadosAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<Turno>> GetByEstadoAsync(EstadoTurno estado);
    }
}
