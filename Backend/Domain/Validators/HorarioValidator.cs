using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class HorarioValidator : EntityValidator<Horario>
    {
        public HorarioValidator()
        {
            RuleFor(h => h.VeterinarioId)
                .NotEmpty().WithMessage("El id del veterinario es requerido");

            RuleFor(h => h.DiaSemana)
                .InclusiveBetween(1, 7).WithMessage("El día de la semana debe estar entre 1 (Lunes) y 7 (Domingo)");

            RuleFor(h => h.TipoHorarioId)
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo de horario válido");

            RuleFor(h => h.HoraFin)
                .GreaterThan(h => h.HoraInicio)
                .WithMessage("La hora de fin debe ser posterior a la hora de inicio");
        }
    }
}
