using Core.Domain.Entities;
using Domain.Others.Utils;
using Domain.Validators;

namespace Domain.Entities
{
    public class Alumno : DomainEntity<string, AlumnoValidator>
    {
        public string Legajo { get; private set; }
        public string Apellido { get; private set; }
        public string Nombre { get; private set; }
        public string FechaIngreso { get; private set; }
        public string DNI { get; private set; }
        public string CorreoElectronico { get; private set; }

        protected Alumno()
        {
        }

        public Alumno(string apellido, string nombre, string dni)
        {
            Id = Guid.NewGuid().ToString();
            Legajo = RandomUtils.GenerarLegajo().ToString();
            Apellido = apellido;
            Nombre = nombre;
            FechaIngreso = DateTime.Now.ToString();
            DNI = dni;
            CorreoElectronico = GenerarCorreoElectronico(nombre, apellido);
        }

        private string GenerarCorreoElectronico(string nombre, string apellido)
        {
            var correo = $"{nombre.Substring(0, 1).ToLower()}{apellido.ToLower()}@institucioneducativa.com";
            return correo;
        }
    }
}
