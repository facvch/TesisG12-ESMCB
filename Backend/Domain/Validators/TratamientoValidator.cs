using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class TratamientoValidator : EntityValidator<Tratamiento>
    {
        public TratamientoValidator()
        {
            RuleFor(t => t.PacienteId)
                .NotEmpty().WithMessage("Debe indicar el paciente");

            RuleFor(t => t.Fecha)
                .NotEmpty().WithMessage("La fecha del tratamiento es requerida")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("La fecha del tratamiento no puede ser futura");

            RuleFor(t => t.Diagnostico)
                .NotEmpty().WithMessage("El diagnóstico es requerido")
                .MaximumLength(500).WithMessage("El diagnóstico no puede superar los 500 caracteres");

            RuleFor(t => t.Descripcion)
                .NotEmpty().WithMessage("La descripción del tratamiento es requerida")
                .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres");

            RuleFor(t => t.Veterinario)
                .NotEmpty().WithMessage("Debe indicar el veterinario responsable")
                .MaximumLength(100).WithMessage("El nombre del veterinario no puede superar los 100 caracteres");

            RuleFor(t => t.Medicacion)
                .MaximumLength(500).WithMessage("La medicación no puede superar los 500 caracteres");
        }
    }
}
