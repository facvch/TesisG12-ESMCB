using Application.ApplicationServices;
using Application.Constants;
using Application.DomainEvents;
using Application.Exceptions;
using Application.Repositories;
using Application.UseCases.DummyEntity.Commands.CreateDummyEntity;
using Core.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Alumno.Commands.CrearAlumno
{
    internal class CrearAlumnoHandler : IRequestCommandHandler<CrearAlumnoCommand, string>
    {
        private readonly ICommandQueryBus _domainBus;
        private readonly IAlumnoRepository _alumnoRepository;
        private readonly IAlumnoApplicationService _alumnoApplicationService;   

        public CrearAlumnoHandler(ICommandQueryBus domainBus, IAlumnoRepository alumnoRepository, IAlumnoApplicationService alumnoApplicationService)
        {
            _domainBus = domainBus ?? throw new ArgumentNullException(nameof(domainBus));
            _alumnoRepository = alumnoRepository ?? throw new ArgumentNullException(nameof(alumnoRepository));
            _alumnoApplicationService = alumnoApplicationService ?? throw new ArgumentNullException(nameof(alumnoApplicationService));
        }

        public async Task<string> Handle(CrearAlumnoCommand request, CancellationToken cancellationToken)
        {
            Domain.Entities.Alumno entity = new Domain.Entities.Alumno(request.Apellido, request.Nombre, request.DNI);

            if (!entity.IsValid) throw new InvalidEntityDataException(entity.GetErrors());

            if (_alumnoApplicationService.AlumnoExist(entity.Legajo)) throw new EntityDoesExistException();

            try
            {
                object createdId = await _alumnoRepository.AddAsync(entity);

                await _domainBus.Publish(entity.To<AlumnoCreado>(), cancellationToken);

                return createdId.ToString();
            }
            catch (Exception ex)
            {
                throw new BussinessException(ApplicationConstants.PROCESS_EXECUTION_EXCEPTION, ex.InnerException);
            }
        }
    }
}
