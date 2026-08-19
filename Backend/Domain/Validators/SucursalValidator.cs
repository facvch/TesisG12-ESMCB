using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class SucursalValidator : EntityValidator<Sucursal>
    {
        public SucursalValidator()
        {
            RuleFor(s => s.Nombre)
                .NotEmpty().WithMessage("El nombre de la sucursal es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");
            RuleFor(s => s.Direccion)
                .NotEmpty().WithMessage("La dirección es requerida")
                .MaximumLength(200).WithMessage("La dirección no puede superar los 200 caracteres");
            RuleFor(s => s.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .MaximumLength(50).WithMessage("El teléfono no puede superar los 50 caracteres");
            RuleFor(s => s.Email)
                .EmailAddress().WithMessage("El correo electrónico no es válido")
                .MaximumLength(100).WithMessage("El correo electrónico no puede superar los 100 caracteres")
                .When(s => !string.IsNullOrEmpty(s.Email));
        }
    }
}
