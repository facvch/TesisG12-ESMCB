using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class RazaValidator : EntityValidator<Raza>
    {
        public RazaValidator()
        {
            RuleFor(r => r.Nombre)
                .NotEmpty().WithMessage("El nombre de la raza es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");

            RuleFor(r => r.EspecieId)
                .GreaterThan(0).WithMessage("Debe seleccionar una especie válida");
        }
    }
}
