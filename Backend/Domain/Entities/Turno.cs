using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa un turno/cita programada en la clínica
    /// </summary>
    public class Turno : DomainEntity<string, TurnoValidator>
    {
        public string PacienteId { get; private set; }
        public string VeterinarioId { get; private set; }
        public int ServicioId { get; private set; }
        public DateTime FechaHora { get; private set; }
        public int DuracionMinutos { get; private set; }
        public EstadoTurno Estado { get; private set; }
        public string Motivo { get; private set; }
        public string Observaciones { get; private set; }
        public DateTime FechaCreacion { get; private set; }

        // Navegación
        public virtual Paciente Paciente { get; private set; }
        public virtual Veterinario Veterinario { get; private set; }
        public virtual Servicio Servicio { get; private set; }

        /// <summary>
        /// Hora de finalización calculada
        /// </summary>
        public DateTime FechaHoraFin => FechaHora.AddMinutes(DuracionMinutos);

        protected Turno() { }

        public Turno(
            string pacienteId,
            string veterinarioId,
            int servicioId,
            DateTime fechaHora,
            int duracionMinutos,
            string motivo = "",
            string observaciones = "") : this()
        {
            Id = Guid.NewGuid().ToString();
            PacienteId = pacienteId;
            VeterinarioId = veterinarioId;
            ServicioId = servicioId;
            FechaHora = fechaHora;
            DuracionMinutos = duracionMinutos;
            Motivo = motivo;
            Observaciones = observaciones;
            Estado = EstadoTurno.Programado;
            FechaCreacion = DateTime.Now;
        }

        public void Reprogramar(DateTime nuevaFechaHora, int duracionMinutos)
        {
            FechaHora = nuevaFechaHora;
            DuracionMinutos = duracionMinutos;
            Estado = EstadoTurno.Reprogramado;
        }

        public void Confirmar()
        {
            Estado = EstadoTurno.Confirmado;
        }

        public void EnCurso()
        {
            Estado = EstadoTurno.EnCurso;
        }

        public void Completar(string observaciones = "")
        {
            Estado = EstadoTurno.Completado;
            if (!string.IsNullOrWhiteSpace(observaciones))
                Observaciones = observaciones;
        }

        public void Cancelar(string motivo = "")
        {
            Estado = EstadoTurno.Cancelado;
            if (!string.IsNullOrWhiteSpace(motivo))
                Observaciones = $"Cancelado: {motivo}";
        }

        public void Ausente()
        {
            Estado = EstadoTurno.Ausente;
        }

        /// <summary>
        /// Verifica si este turno se superpone con otro rango horario
        /// </summary>
        public bool SeSuperponeCon(DateTime otraFechaHora, int otraDuracion)
        {
            var otroFin = otraFechaHora.AddMinutes(otraDuracion);
            return FechaHora < otroFin && FechaHoraFin > otraFechaHora;
        }
    }

    /// <summary>
    /// Estados posibles de un turno
    /// </summary>
    public enum EstadoTurno
    {
        Programado = 0,
        Confirmado = 1,
        EnCurso = 2,
        Completado = 3,
        Cancelado = 4,
        Ausente = 5,
        Reprogramado = 6
    }
}
