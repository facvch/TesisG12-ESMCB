using System.ComponentModel.DataAnnotations;

namespace BlazorFrontEnd.Models
{    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string NombreUsuario { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email incorrecto")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre completo es requerido")]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un Rol")]
        public int RolId { get; set; }
    }
}
