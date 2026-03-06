using Core.Domain.Entities;
using Domain.Validators;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Entities
{
    /// <summary>
    /// Roles del sistema: Admin, Veterinario, Recepcionista, etc.
    /// </summary>
    public class Rol : DomainEntity<int, RolValidator>
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public bool Activo { get; private set; }

        protected Rol() { }

        public Rol(string nombre, string descripcion = "") : this()
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

    /// <summary>
    /// Usuario del sistema con hash de contraseña
    /// </summary>
    public class Usuario : DomainEntity<string, UsuarioValidator>
    {
        public string NombreUsuario { get; private set; }
        public string Email { get; private set; }
        public string NombreCompleto { get; private set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public int RolId { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? UltimoLogin { get; private set; }
        public bool Activo { get; private set; }

        // Navegación
        public virtual Rol Rol { get; private set; }

        protected Usuario() { }

        public Usuario(string nombreUsuario, string email, string nombreCompleto,
            string password, int rolId) : this()
        {
            Id = Guid.NewGuid().ToString();
            NombreUsuario = nombreUsuario;
            Email = email;
            NombreCompleto = nombreCompleto;
            RolId = rolId;
            FechaCreacion = DateTime.Now;
            Activo = true;

            SetPassword(password);
        }

        public void SetPassword(string password)
        {
            var salt = GenerateSalt();
            PasswordSalt = Convert.ToBase64String(salt);
            PasswordHash = HashPassword(password, salt);
        }

        public bool VerifyPassword(string password)
        {
            var salt = Convert.FromBase64String(PasswordSalt);
            var hash = HashPassword(password, salt);
            return hash == PasswordHash;
        }

        public void RegistrarLogin() => UltimoLogin = DateTime.Now;

        public void Actualizar(string email, string nombreCompleto)
        {
            Email = email;
            NombreCompleto = nombreCompleto;
        }

        public void CambiarRol(int rolId) => RolId = rolId;
        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;

        private static byte[] GenerateSalt()
        {
            var salt = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }
}
