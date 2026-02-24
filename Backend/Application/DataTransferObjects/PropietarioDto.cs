namespace Application.DataTransferObjects
{
    public class PropietarioDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto { get; set; }
        public string DNI { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public int CantidadMascotas { get; set; }
    }
}
