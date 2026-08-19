namespace Application.DataTransferObjects
{
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
        public int? SucursalId { get; set; }
        public string? SucursalNombre { get; set; }
        public List<HorarioDto> Horarios { get; set; } = new();
        public string DisponibilidadActual { get; set; } = "No Disponible"; // Disponible, No Disponible, Guardia
    }
}
