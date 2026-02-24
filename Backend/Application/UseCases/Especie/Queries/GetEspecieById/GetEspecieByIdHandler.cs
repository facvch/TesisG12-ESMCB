using Application.DataTransferObjects;
using Application.Repositories;
using Application.Exceptions;
using Core.Application;

namespace Application.UseCases.Especie.Queries.GetEspecieById
{
    internal class GetEspecieByIdHandler(IEspecieRepository repository) 
        : IRequestQueryHandler<GetEspecieByIdQuery, EspecieDto>
    {
        private readonly IEspecieRepository _repository = repository 
            ?? throw new ArgumentNullException(nameof(repository));

        public async Task<EspecieDto> Handle(GetEspecieByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.FindOneAsync(request.Id);

            if (entity == null)
                throw new EntityDoesNotExistException($"No se encontró la especie con Id {request.Id}");

            return new EspecieDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Activo = entity.Activo
            };
        }
    }
}
