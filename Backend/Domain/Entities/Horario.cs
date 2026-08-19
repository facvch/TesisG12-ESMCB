using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa un rango de horario de disponibilidad asignado a un profesional
    /// </summary>
    public class Horario : DomainEntity<string, HorarioValidator>
    {
        public string VeterinarioId { get; private set; }
        public int DiaSemana { get; private set; } // 1=Lunes, 2=Martes, 3=Miércoles, 4=Jueves, 5=Viernes, 6=Sábado, 7=Domingo
        public TimeSpan HoraInicio { get; private set; }
        public TimeSpan HoraFin { get; private set; }
        public int TipoHorarioId { get; private set; }
        public bool Activo { get; private set; }

        // Navegación
        public virtual Veterinario Veterinario { get; private set; }
        public virtual TipoHorario TipoHorario { get; private set; }

        protected Horario() { }

        public Horario(
            string veterinarioId,
            int diaSemana,
            TimeSpan horaInicio,
            TimeSpan horaFin,
            int tipoHorarioId)
        {
            Id = Guid.NewGuid().ToString();
            VeterinarioId = veterinarioId;
            DiaSemana = diaSemana;
            HoraInicio = horaInicio;
            HoraFin = horaFin;
            TipoHorarioId = tipoHorarioId;
            Activo = true;
        }

        public void Actualizar(int diaSemana, TimeSpan horaInicio, TimeSpan horaFin, int tipoHorarioId)
        {
            DiaSemana = diaSemana;
            HoraInicio = horaInicio;
            HoraFin = horaFin;
            TipoHorarioId = tipoHorarioId;
        }

        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;

        public static string CalcularDisponibilidad(IEnumerable<Horario> horarios, DateTime now)
        {
            if (horarios == null || !horarios.Any()) return "No Disponible";

            int currentDayIso = now.DayOfWeek switch
            {
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                DayOfWeek.Sunday => 7,
                _ => 1
            };

            var currentTime = now.TimeOfDay;

            foreach (var h in horarios.Where(x => x.Activo && x.DiaSemana == currentDayIso))
            {
                if (currentTime >= h.HoraInicio && currentTime <= h.HoraFin)
                {
                    if (h.TipoHorarioId == 2)
                    {
                        return "Guardia";
                    }
                    return "Disponible";
                }
            }

            return "No Disponible";
        }
    }
}
