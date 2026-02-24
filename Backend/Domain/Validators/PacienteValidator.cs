using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class PacienteValidator : EntityValidator<Paciente>
    {
        public PacienteValidator()
        {
            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("El nombre del paciente es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");

            RuleFor(p => p.EspecieId)
                .GreaterThan(0).WithMessage("Debe seleccionar una especie válida");

            RuleFor(p => p.PropietarioId)
                .NotEmpty().WithMessage("Debe asignar un propietario al paciente");

            RuleFor(p => p.Sexo)
                .NotEmpty().WithMessage("El sexo es requerido")
                .Must(s => s == "M" || s == "H").WithMessage("El sexo debe ser 'M' (Macho) o 'H' (Hembra)");

            RuleFor(p => p.FechaNacimiento)
                .LessThanOrEqualTo(DateTime.Now).When(p => p.FechaNacimiento.HasValue)
                .WithMessage("La fecha de nacimiento no puede ser futura");
        }
    }
}
