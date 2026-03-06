namespace Application.DataTransferObjects
{
    public class AuditLogDto
    {
        public string Id { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string Accion { get; set; }
        public string Entidad { get; set; }
        public string EntidadId { get; set; }
        public string Descripcion { get; set; }
        public string IpOrigen { get; set; }
        public DateTime Fecha { get; set; }
        public int StatusCode { get; set; }
    }

    public class NotificacionDto
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string Tipo { get; set; }
        public string EntidadRelacionada { get; set; }
        public string EntidadId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
        public DateTime? FechaLectura { get; set; }
    }
}
