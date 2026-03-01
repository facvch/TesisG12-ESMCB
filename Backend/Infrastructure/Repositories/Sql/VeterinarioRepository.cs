using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class VeterinarioRepository : BaseRepository<Veterinario>, IVeterinarioRepository
    {
        public VeterinarioRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Veterinario>> GetActivosAsync()
        {
            return await Repository.Where(v => v.Activo).ToListAsync();
        }

        public async Task<Veterinario> GetByMatriculaAsync(string matricula)
        {
            return await Repository.FirstOrDefaultAsync(v => v.Matricula == matricula);
        }

        public async Task<IEnumerable<Veterinario>> SearchByNombreAsync(string nombre)
        {
            var search = nombre.ToLower();
            return await Repository
                .Where(v => v.Nombre.ToLower().Contains(search) || v.Apellido.ToLower().Contains(search))
                .ToListAsync();
        }
    }
}
