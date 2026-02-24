using Core.Application;
using System.ComponentModel.DataAnnotations;

namespace Application.UseCases.Especie.Commands.CreateEspecie
{
    public class CreateEspecieCommand : IRequestCommand<int>
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }
        
        [MaxLength(200)]
        public string Descripcion { get; set; }
    }
}
