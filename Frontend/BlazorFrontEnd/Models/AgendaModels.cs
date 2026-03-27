using System.ComponentModel.DataAnnotations;

namespace BlazorFrontEnd.Models
{
    public class TurnoDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Debe seleccionar un paciente")]
        public string PacienteId { get; set; } = string.Empty;
        public string PacienteNombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Debe seleccionar un veterinario")]
        public string VeterinarioId { get; set; } = string.Empty;
        public string VeterinarioNombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Debe seleccionar un servicio")]
        public int ServicioId { get; set; }
        public string ServicioNombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La fecha y hora del turno son requeridas")]
        public DateTime FechaHora { get; set; } = DateTime.Today.AddHours(9);
        public DateTime FechaHoraFin { get; set; }
        public int DuracionMinutos { get; set; }
        
        public string Estado { get; set; } = "Programado"; // Programado, Completado, Cancelado, Reprogramado
        
        [Required(ErrorMessage = "El motivo es requerido")]
        public string Motivo { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class VeterinarioDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class ServicioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
    }
}
