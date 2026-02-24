using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ApplicationServices
{
    public class AlumnoApplicationService : IAlumnoApplicationService
    {
        private readonly IAlumnoRepository _alumnoRepository;
        public AlumnoApplicationService(IAlumnoRepository alumnoRepository)
        {
            _alumnoRepository = alumnoRepository ?? throw new ArgumentNullException(nameof(alumnoRepository));
        }


        public bool AlumnoExist(object legajo)
        {
            legajo = legajo ?? throw new ArgumentNullException(nameof(legajo));

            var response = _alumnoRepository.FindOne(legajo);

            return response != null;
        }
    }
}
