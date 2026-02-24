using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class PropietarioValidator : EntityValidator<Propietario>
    {
        public PropietarioValidator()
        {
            RuleFor(p => p.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");

            RuleFor(p => p.Apellido)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres");

            RuleFor(p => p.DNI)
                .NotEmpty().WithMessage("El DNI es requerido")
                .MaximumLength(20).WithMessage("El DNI no puede superar los 20 caracteres");

            RuleFor(p => p.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .MaximumLength(20).WithMessage("El teléfono no puede superar los 20 caracteres");

            RuleFor(p => p.Email)
                .MaximumLength(100).WithMessage("El email no puede superar los 100 caracteres")
                .EmailAddress().When(p => !string.IsNullOrEmpty(p.Email))
                .WithMessage("El formato del email no es válido");
        }
    }
}
