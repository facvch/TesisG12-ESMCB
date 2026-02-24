using Application.DataTransferObjects;
using Core.Application;

namespace Application.UseCases.Especie.Queries.GetAllEspecies
{
    public class GetAllEspeciesQuery : QueryRequest<QueryResult<EspecieDto>>
    {
        public bool SoloActivas { get; set; } = true;
    }
}
