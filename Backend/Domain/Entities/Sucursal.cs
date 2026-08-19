using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    public class Sucursal : DomainEntity<int, SucursalValidator>
    {
        public string Nombre { get; private set; }
        public string Direccion { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public bool Activa { get; private set; }

        protected Sucursal() { }

        public Sucursal(string nombre, string direccion, string telefono, string email = "") : this()
        {
            Nombre = nombre;
            Direccion = direccion;
            Telefono = telefono;
            Email = email;
            Activa = true;
        }

        public void Actualizar(string nombre, string direccion, string telefono, string email)
        {
            Nombre = nombre;
            Direccion = direccion;
            Telefono = telefono;
            Email = email;
        }

        public void Desactivar() => Activa = false;
        public void Activar() => Activa = true;
    }
}
