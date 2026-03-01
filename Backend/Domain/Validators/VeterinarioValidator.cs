using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class VeterinarioValidator : EntityValidator<Veterinario>
    {
        public VeterinarioValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres");

            RuleFor(v => v.Apellido)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MaximumLength(50).WithMessage("El apellido no puede superar los 50 caracteres");

            RuleFor(v => v.Matricula)
                .NotEmpty().WithMessage("La matrícula es requerida")
                .MaximumLength(20).WithMessage("La matrícula no puede superar los 20 caracteres");

            RuleFor(v => v.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .MaximumLength(20).WithMessage("El teléfono no puede superar los 20 caracteres");

            RuleFor(v => v.Email)
                .MaximumLength(100).WithMessage("El email no puede superar los 100 caracteres")
                .EmailAddress().When(v => !string.IsNullOrEmpty(v.Email))
                .WithMessage("El formato del email no es válido");

            RuleFor(v => v.Especialidad)
                .MaximumLength(100).WithMessage("La especialidad no puede superar los 100 caracteres");
        }
    }
}
