using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class CategoriaValidator : EntityValidator<Categoria>
    {
        public CategoriaValidator()
        {
            RuleFor(c => c.Nombre)
                .NotEmpty().WithMessage("El nombre de la categoría es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");
            RuleFor(c => c.Descripcion)
                .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres");
        }
    }
}
