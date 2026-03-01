namespace Application.DataTransferObjects
{
    public class VeterinarioDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto { get; set; }
        public string Matricula { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Especialidad { get; set; }
        public bool Activo { get; set; }
    }
}
