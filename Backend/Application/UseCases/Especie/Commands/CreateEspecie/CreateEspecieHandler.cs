using Application.Repositories;
using Core.Application;
using Application.Exceptions;

namespace Application.UseCases.Especie.Commands.CreateEspecie
{
    internal sealed class CreateEspecieHandler(IEspecieRepository especieRepository) 
        : IRequestCommandHandler<CreateEspecieCommand, int>
    {
        private readonly IEspecieRepository _repository = especieRepository 
            ?? throw new ArgumentNullException(nameof(especieRepository));

        public async Task<int> Handle(CreateEspecieCommand request, CancellationToken cancellationToken)
        {
            var entity = new Domain.Entities.Especie(request.Nombre, request.Descripcion ?? "");

            if (!entity.IsValid)
                throw new InvalidEntityDataException(entity.GetErrors());

            var existingEspecie = await _repository.GetByNombreAsync(request.Nombre);
            if (existingEspecie != null)
                throw new EntityDoesExistException($"Ya existe una especie con el nombre '{request.Nombre}'");

            var createdId = await _repository.AddAsync(entity);
            return (int)createdId;
        }
    }
}
