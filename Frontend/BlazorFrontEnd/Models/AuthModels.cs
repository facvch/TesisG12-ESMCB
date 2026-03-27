namespace BlazorFrontEnd.Models
{
    public class LoginRequest
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracion { get; set; }
        public UsuarioDto Usuario { get; set; } = new();
    }

    public class UsuarioDto
    {
        public string Id { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Activo { get; set; }
    }
}
