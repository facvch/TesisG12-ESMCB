using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;

namespace Infrastructure.Repositories.Sql
{
    internal class AlumnoRepository : BaseRepository<Alumno>, IAlumnoRepository
    {
        public AlumnoRepository(StoreDbContext context) : base(context)
        {

        }
    }
}
