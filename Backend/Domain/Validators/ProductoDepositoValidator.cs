using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class ProductoDepositoValidator : EntityValidator<ProductoDeposito>
    {
        public ProductoDepositoValidator()
        {
            RuleFor(pd => pd.ProductoId)
                .NotEmpty().WithMessage("El producto es requerido");
            RuleFor(pd => pd.DepositoId)
                .GreaterThan(0).WithMessage("El depósito es requerido");
            RuleFor(pd => pd.StockActual)
                .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");
        }
    }
}
