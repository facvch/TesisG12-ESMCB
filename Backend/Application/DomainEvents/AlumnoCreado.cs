using Core.Application;

namespace Application.DomainEvents
{
    internal class AlumnoCreado : DomainEvent
    {
        public string Legajo { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string FechaIngreso { get; set; }
        public string DNI { get; set; }
        public string CorreoElectronico { get; set; }
    }
}
