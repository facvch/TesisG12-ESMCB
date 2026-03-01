using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class MarcaValidator : EntityValidator<Marca>
    {
        public MarcaValidator()
        {
            RuleFor(m => m.Nombre)
                .NotEmpty().WithMessage("El nombre de la marca es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");
        }
    }
}
