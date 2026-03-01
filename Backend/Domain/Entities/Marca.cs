using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    public class Marca : DomainEntity<int, MarcaValidator>
    {
        public string Nombre { get; private set; }
        public bool Activo { get; private set; }

        protected Marca() { }

        public Marca(string nombre) : this()
        {
            Nombre = nombre;
            Activo = true;
        }

        public void Actualizar(string nombre) => Nombre = nombre;
        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
    }
}
