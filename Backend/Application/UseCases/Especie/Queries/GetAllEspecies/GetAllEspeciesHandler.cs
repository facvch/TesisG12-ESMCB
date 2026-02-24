using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;

namespace Application.UseCases.Especie.Queries.GetAllEspecies
{
    internal class GetAllEspeciesHandler(IEspecieRepository repository) 
        : IRequestQueryHandler<GetAllEspeciesQuery, QueryResult<EspecieDto>>
    {
        private readonly IEspecieRepository _repository = repository 
            ?? throw new ArgumentNullException(nameof(repository));

        public async Task<QueryResult<EspecieDto>> Handle(GetAllEspeciesQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Domain.Entities.Especie> entities;
            
            if (request.SoloActivas)
                entities = await _repository.GetActivasAsync();
            else
                entities = await _repository.FindAllAsync();

            var dtos = entities.Select(e => new EspecieDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion,
                Activo = e.Activo
            }).ToList();

            return new QueryResult<EspecieDto>(dtos, dtos.Count, request.PageIndex, request.PageSize);
        }
    }
}
