using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    public class MetodoPago : DomainEntity<int, MetodoPagoValidator>
    {
        public string Nombre { get; private set; }  // Efectivo, Tarjeta Débito, Tarjeta Crédito, Transferencia
        public bool Activo { get; private set; }

        protected MetodoPago() { }

        public MetodoPago(string nombre) : this()
        {
            Nombre = nombre;
            Activo = true;
        }

        public void Actualizar(string nombre) => Nombre = nombre;
        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
    }
}
