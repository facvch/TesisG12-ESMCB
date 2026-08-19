using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IHorarioRepository : IRepository<Horario>
    {
        Task<IEnumerable<Horario>> GetByVeterinarioIdAsync(string veterinarioId);
        Task<IEnumerable<Horario>> GetActivosAsync();
        Task DeleteByVeterinarioIdAsync(string veterinarioId);
    }
}
