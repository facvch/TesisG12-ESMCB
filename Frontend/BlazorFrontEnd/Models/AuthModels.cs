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
        public string? FotoUrl { get; set; }
        public string? VeterinarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Activo { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string NuevaPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
    }

    public class UpdatePhotoRequest
    {
        public string? FotoBase64 { get; set; }
    }

    public class SaveVeterinarioRequest
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Matricula { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Especialidad { get; set; }
    }

    public class VeterinarioPerfilDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class AuditLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public string? EntidadId { get; set; }
        public string? Descripcion { get; set; }
        public string? IpOrigen { get; set; }
        public DateTime Fecha { get; set; }
        public int StatusCode { get; set; }
    }
}
