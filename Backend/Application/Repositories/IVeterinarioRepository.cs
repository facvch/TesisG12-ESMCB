using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IVeterinarioRepository : IRepository<Veterinario>
    {
        Task<Veterinario> GetByMatriculaAsync(string matricula);
        Task<IEnumerable<Veterinario>> GetActivosAsync();
        Task<IEnumerable<Veterinario>> SearchByNombreAsync(string nombre);
    }
}
