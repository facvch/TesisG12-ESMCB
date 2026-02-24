using Core.Application;
using System.ComponentModel.DataAnnotations;

namespace Application.UseCases.Especie.Commands.UpdateEspecie
{
    public class UpdateEspecieCommand : IRequestCommand
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }
        
        [MaxLength(200)]
        public string Descripcion { get; set; }
    }
}
