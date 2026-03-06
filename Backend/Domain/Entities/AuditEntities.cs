using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Registro de auditoría - guarda cada acción realizada en el sistema
    /// </summary>
    public class AuditLog : DomainEntity<string, AuditLogValidator>
    {
        public string UsuarioId { get; private set; }
        public string NombreUsuario { get; private set; }
        public string Accion { get; private set; }        // POST, PUT, DELETE
        public string Entidad { get; private set; }        // Nombre del controller/entidad
        public string EntidadId { get; private set; }      // Id del recurso afectado
        public string Descripcion { get; private set; }    // Detalle de la acción
        public string IpOrigen { get; private set; }
        public DateTime Fecha { get; private set; }
        public int StatusCode { get; private set; }

        protected AuditLog() { }

        public AuditLog(string usuarioId, string nombreUsuario, string accion,
            string entidad, string entidadId, string descripcion,
            string ipOrigen, int statusCode) : this()
        {
            Id = Guid.NewGuid().ToString();
            UsuarioId = usuarioId ?? "anonimo";
            NombreUsuario = nombreUsuario ?? "Anónimo";
            Accion = accion;
            Entidad = entidad;
            EntidadId = entidadId ?? "";
            Descripcion = descripcion;
            IpOrigen = ipOrigen ?? "";
            Fecha = DateTime.Now;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Notificación del sistema (vacunas pendientes, stock bajo, etc.)
    /// </summary>
    public class Notificacion : DomainEntity<string, NotificacionValidator>
    {
        public string Titulo { get; private set; }
        public string Mensaje { get; private set; }
        public TipoNotificacion Tipo { get; private set; }
        public string EntidadRelacionada { get; private set; }
        public string EntidadId { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public bool Leida { get; private set; }
        public DateTime? FechaLectura { get; private set; }

        protected Notificacion() { }

        public Notificacion(string titulo, string mensaje, TipoNotificacion tipo,
            string entidadRelacionada = "", string entidadId = "") : this()
        {
            Id = Guid.NewGuid().ToString();
            Titulo = titulo;
            Mensaje = mensaje;
            Tipo = tipo;
            EntidadRelacionada = entidadRelacionada;
            EntidadId = entidadId;
            FechaCreacion = DateTime.Now;
            Leida = false;
        }

        public void MarcarLeida()
        {
            Leida = true;
            FechaLectura = DateTime.Now;
        }
    }

    public enum TipoNotificacion
    {
        VacunaPendiente = 0,
        StockBajo = 1,
        TurnoProximo = 2,
        TratamientoVencido = 3,
        Sistema = 4
    }
}
