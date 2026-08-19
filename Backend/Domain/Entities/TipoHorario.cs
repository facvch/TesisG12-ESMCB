using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa un tipo de horario de trabajo (ej: Normal, Guardia)
    /// </summary>
    public class TipoHorario : DomainEntity<int, TipoHorarioValidator>
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public bool Activo { get; private set; }

        public virtual ICollection<Horario> Horarios { get; private set; }

        protected TipoHorario()
        {
            Horarios = new List<Horario>();
        }

        public TipoHorario(int id, string nombre, string descripcion = "") : this()
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Activo = true;
        }

        public TipoHorario(string nombre, string descripcion = "") : this()
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

        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
    }
}
