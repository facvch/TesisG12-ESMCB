using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class EspecieValidator : EntityValidator<Especie>
    {
        public EspecieValidator()
        {
            RuleFor(e => e.Nombre)
                .NotEmpty().WithMessage("El nombre de la especie es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres");
        }
    }
}
