using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa una raza dentro de una especie (Golden Retriever, Siamés, etc.)
    /// </summary>
    public class Raza : DomainEntity<int, RazaValidator>
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public int EspecieId { get; private set; }
        public bool Activo { get; private set; }

        // Navegación
        public virtual Especie Especie { get; private set; }
        public virtual ICollection<Paciente> Pacientes { get; private set; }

        protected Raza()
        {
            Pacientes = new List<Paciente>();
        }

        public Raza(string nombre, int especieId, string descripcion = "") : this()
        {
            Nombre = nombre;
            EspecieId = especieId;
            Descripcion = descripcion;
            Activo = true;
        }

        public void Actualizar(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;
        }

        public void CambiarEspecie(int especieId)
        {
            EspecieId = especieId;
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
