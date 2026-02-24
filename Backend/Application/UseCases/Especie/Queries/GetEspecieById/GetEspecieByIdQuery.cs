using Application.DataTransferObjects;
using Core.Application;

namespace Application.UseCases.Especie.Queries.GetEspecieById
{
    public class GetEspecieByIdQuery : QueryRequest<EspecieDto>
    {
        public int Id { get; set; }
    }
}
