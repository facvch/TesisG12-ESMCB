using Application.Repositories;
using Application.Exceptions;
using Core.Application;

namespace Application.UseCases.Especie.Commands.DeleteEspecie
{
    internal sealed class DeleteEspecieHandler(IEspecieRepository repository) 
        : IRequestCommandHandler<DeleteEspecieCommand>
    {
        private readonly IEspecieRepository _repository = repository 
            ?? throw new ArgumentNullException(nameof(repository));

        public async Task Handle(DeleteEspecieCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FindOneAsync(request.Id);
            
            if (entity == null)
                throw new EntityDoesNotExistException($"No se encontró la especie con Id {request.Id}");

            // Soft delete - desactivar en lugar de eliminar
            entity.Desactivar();
            _repository.Update(request.Id, entity);
        }
    }
}
