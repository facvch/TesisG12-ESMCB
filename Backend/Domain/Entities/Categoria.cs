using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    public class Categoria : DomainEntity<int, CategoriaValidator>
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public bool Activo { get; private set; }

        protected Categoria() { }

        public Categoria(string nombre, string descripcion = "") : this()
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
