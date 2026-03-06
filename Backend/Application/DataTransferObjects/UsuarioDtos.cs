namespace Application.DataTransferObjects
{
    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
    }

    public class UsuarioDto
    {
        public string Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Email { get; set; }
        public string NombreCompleto { get; set; }
        public int RolId { get; set; }
        public string RolNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Activo { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiracion { get; set; }
        public UsuarioDto Usuario { get; set; }
    }
}
