using Core.Application;

namespace Application.UseCases.Alumno.Commands.CrearAlumno
{
    public class CrearAlumnoCommand : IRequestCommand<string>
    {
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string DNI { get; set; }
    }
}
