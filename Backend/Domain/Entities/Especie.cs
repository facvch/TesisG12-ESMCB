using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa una especie animal (Canino, Felino, Ave, etc.)
    /// </summary>
    public class Especie : DomainEntity<int, EspecieValidator>
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public bool Activo { get; private set; }

        // Navegación
        public virtual ICollection<Raza> Razas { get; private set; }
        public virtual ICollection<Paciente> Pacientes { get; private set; }

        protected Especie()
        {
            Razas = new List<Raza>();
            Pacientes = new List<Paciente>();
        }

        public Especie(string nombre, string descripcion = "") : this()
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Activo = true;
        }

        public void Actualizar(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public void Activar()
        {
            Activo = true;
        }
    }
}
