using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class TurnoValidator : EntityValidator<Turno>
    {
        public TurnoValidator()
        {
            RuleFor(t => t.PacienteId)
                .NotEmpty().WithMessage("Debe indicar el paciente");

            RuleFor(t => t.VeterinarioId)
                .NotEmpty().WithMessage("Debe indicar el veterinario");

            RuleFor(t => t.ServicioId)
                .GreaterThan(0).WithMessage("Debe seleccionar un servicio válido");

            RuleFor(t => t.FechaHora)
                .NotEmpty().WithMessage("La fecha y hora del turno es requerida")
                .GreaterThan(DateTime.Now.AddMinutes(-5)).WithMessage("La fecha del turno no puede ser en el pasado");

            RuleFor(t => t.DuracionMinutos)
                .GreaterThan(0).WithMessage("La duración debe ser mayor a 0 minutos")
                .LessThanOrEqualTo(480).WithMessage("La duración no puede superar las 8 horas");

            RuleFor(t => t.Motivo)
                .MaximumLength(200).WithMessage("El motivo no puede superar los 200 caracteres");

            RuleFor(t => t.Observaciones)
                .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres");
        }
    }
}
