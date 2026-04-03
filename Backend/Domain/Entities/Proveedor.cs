using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    public class Proveedor : DomainEntity<string, ProveedorValidator>
    {
        public string RazonSocial { get; private set; }
        public string CUIT { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public string Direccion { get; private set; }
        public string Contacto { get; private set; }
        public bool Activo { get; private set; }

        protected Proveedor() { }

        public Proveedor(string razonSocial, string cuit, string telefono,
            string email = "", string direccion = "", string contacto = "") : this()
        {
            Id = Guid.NewGuid().ToString();
            RazonSocial = razonSocial;
            CUIT = cuit;
            Telefono = telefono;
            Email = email;
            Direccion = direccion;
            Contacto = contacto;
            Activo = true;
        }

        public void Actualizar(string razonSocial, string cuit, string telefono, string email, string direccion, string contacto)
        {
            RazonSocial = razonSocial;
            CUIT = cuit;
            Telefono = telefono;
            Email = email;
            Direccion = direccion;
            Contacto = contacto;
        }

        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
    }
}
