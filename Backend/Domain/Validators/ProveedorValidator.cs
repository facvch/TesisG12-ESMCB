using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class ProveedorValidator : EntityValidator<Proveedor>
    {
        public ProveedorValidator()
        {
            RuleFor(p => p.RazonSocial)
                .NotEmpty().WithMessage("La razón social es requerida")
                .MaximumLength(150).WithMessage("La razón social no puede superar los 150 caracteres");
            RuleFor(p => p.CUIT)
                .NotEmpty().WithMessage("El CUIT es requerido")
                .MaximumLength(13).WithMessage("El CUIT no puede superar los 13 caracteres");
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
