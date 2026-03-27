using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class PacienteRepository : BaseRepository<Paciente>, IPacienteRepository
    {
        public PacienteRepository(StoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Paciente>> GetPacientesExpandidosAsync()
        {
            return await Repository
                .Include(p => p.Especie)
                .Include(p => p.Raza)
                .Include(p => p.Propietario)
                .ToListAsync();
        }

        public async Task<IEnumerable<Paciente>> GetActivosAsync()
        {
            return await Repository.Where(p => p.Activo).ToListAsync();
        }

        public async Task<IEnumerable<Paciente>> GetByEspecieIdAsync(int especieId)
        {
            return await Repository.Where(p => p.EspecieId == especieId && p.Activo).ToListAsync();
        }

        public async Task<IEnumerable<Paciente>> GetByPropietarioIdAsync(string propietarioId)
        {
            return await Repository.Where(p => p.PropietarioId == propietarioId && p.Activo).ToListAsync();
        }

        public async Task<IEnumerable<Paciente>> SearchByNombreAsync(string nombre)
        {
            var nombreLower = nombre.ToLower();
            return await Repository
                .Where(p => p.Nombre.ToLower().Contains(nombreLower) && p.Activo)
                .ToListAsync();
        }
    }
}
