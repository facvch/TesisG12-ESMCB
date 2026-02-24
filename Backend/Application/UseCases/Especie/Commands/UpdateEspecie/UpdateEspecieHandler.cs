using Application.Repositories;
using Application.Exceptions;
using Core.Application;

namespace Application.UseCases.Especie.Commands.UpdateEspecie
{
    internal sealed class UpdateEspecieHandler(IEspecieRepository repository) 
        : IRequestCommandHandler<UpdateEspecieCommand>
    {
        private readonly IEspecieRepository _repository = repository 
            ?? throw new ArgumentNullException(nameof(repository));

        public async Task Handle(UpdateEspecieCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FindOneAsync(request.Id);
            
            if (entity == null)
                throw new EntityDoesNotExistException($"No se encontró la especie con Id {request.Id}");

            entity.Actualizar(request.Nombre, request.Descripcion ?? "");

            if (!entity.IsValid)
                throw new InvalidEntityDataException(entity.GetErrors());

            _repository.Update(request.Id, entity);
        }
    }
}
