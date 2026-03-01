using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class MovimientoStockValidator : EntityValidator<MovimientoStock>
    {
        public MovimientoStockValidator()
        {
            RuleFor(m => m.ProductoId)
                .NotEmpty().WithMessage("Debe indicar el producto");
            RuleFor(m => m.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");
            RuleFor(m => m.Motivo)
                .MaximumLength(200).WithMessage("El motivo no puede superar los 200 caracteres");
            RuleFor(m => m.Referencia)
                .MaximumLength(50).WithMessage("La referencia no puede superar los 50 caracteres");
        }
    }
}
