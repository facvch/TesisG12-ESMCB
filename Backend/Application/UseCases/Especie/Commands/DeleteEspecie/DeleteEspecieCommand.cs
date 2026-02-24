using Core.Application;

namespace Application.UseCases.Especie.Commands.DeleteEspecie
{
    public class DeleteEspecieCommand : IRequestCommand
    {
        public int Id { get; set; }
    }
}
