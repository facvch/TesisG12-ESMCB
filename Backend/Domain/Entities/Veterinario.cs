using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa un veterinario que trabaja en la clínica
    /// </summary>
    public class Veterinario : DomainEntity<string, VeterinarioValidator>
    {
        public string Nombre { get; private set; }
        public string Apellido { get; private set; }
        public string Matricula { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public string Especialidad { get; private set; }
        public int SucursalId { get; private set; }
        public bool Activo { get; private set; }

        // Navegación
        public virtual Sucursal Sucursal { get; private set; }
        public virtual ICollection<Turno> Turnos { get; private set; }
        public virtual ICollection<Horario> Horarios { get; private set; }

        public string NombreCompleto => $"{Nombre} {Apellido}";

        protected Veterinario()
        {
            Turnos = new List<Turno>();
            Horarios = new List<Horario>();
        }

        public Veterinario(
            string nombre,
            string apellido,
            string matricula,
            string telefono,
            string email = "",
            string especialidad = "",
            int sucursalId = 0) : this()
        {
            Id = Guid.NewGuid().ToString();
            Nombre = nombre;
            Apellido = apellido;
            Matricula = matricula;
            Telefono = telefono;
            Email = email;
            Especialidad = especialidad;
            SucursalId = sucursalId;
            Activo = true;
        }

        public void AsignarSucursal(int sucursalId) => SucursalId = sucursalId;

        public void Actualizar(string nombre, string apellido, string telefono, string email, string especialidad)
        {
            Nombre = nombre;
            Apellido = apellido;
            Telefono = telefono;
            Email = email;
            Especialidad = especialidad;
        }

        public void Actualizar(string nombre, string apellido, string matricula, string telefono, string email, string especialidad)
        {
            Nombre = nombre;
            Apellido = apellido;
            Matricula = matricula;
            Telefono = telefono;
            Email = email;
            Especialidad = especialidad;
        }

        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
    }
}
