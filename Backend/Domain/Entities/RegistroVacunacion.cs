using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa un registro de vacunación aplicada a un paciente
    /// </summary>
    public class RegistroVacunacion : DomainEntity<string, RegistroVacunacionValidator>
    {
        public string PacienteId { get; private set; }
        public int VacunaId { get; private set; }
        public DateTime FechaAplicacion { get; private set; }
        public DateTime? FechaProximaDosis { get; private set; }
        public string Veterinario { get; private set; }
        public string NroLote { get; private set; }
        public string Observaciones { get; private set; }

        // Navegación
        public virtual Paciente Paciente { get; private set; }
        public virtual Vacuna Vacuna { get; private set; }

        protected RegistroVacunacion() { }

        public RegistroVacunacion(
            string pacienteId,
            int vacunaId,
            DateTime fechaAplicacion,
            string veterinario,
            string nroLote = "",
            DateTime? fechaProximaDosis = null,
            string observaciones = "") : this()
        {
            Id = Guid.NewGuid().ToString();
            PacienteId = pacienteId;
            VacunaId = vacunaId;
            FechaAplicacion = fechaAplicacion;
            Veterinario = veterinario;
            NroLote = nroLote;
            FechaProximaDosis = fechaProximaDosis;
            Observaciones = observaciones;
        }

        public void Actualizar(DateTime fechaAplicacion, string veterinario, string nroLote, DateTime? fechaProximaDosis, string observaciones)
        {
            FechaAplicacion = fechaAplicacion;
            Veterinario = veterinario;
            NroLote = nroLote;
            FechaProximaDosis = fechaProximaDosis;
            Observaciones = observaciones;
        }
    }
}
