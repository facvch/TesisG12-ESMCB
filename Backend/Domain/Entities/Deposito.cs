using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    public class Deposito : DomainEntity<int, DepositoValidator>
    {
        public string Nombre { get; private set; }
        public string Ubicacion { get; private set; }
        public bool Activo { get; private set; }

        protected Deposito() { }

        public Deposito(string nombre, string ubicacion = "") : this()
        {
            Nombre = nombre;
            Ubicacion = ubicacion;
            Activo = true;
        }

        public void Actualizar(string nombre, string ubicacion)
        {
            Nombre = nombre;
            Ubicacion = ubicacion;
        }

        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
    }
}
