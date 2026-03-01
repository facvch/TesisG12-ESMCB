using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class DepositoValidator : EntityValidator<Deposito>
    {
        public DepositoValidator()
        {
            RuleFor(d => d.Nombre)
                .NotEmpty().WithMessage("El nombre del depósito es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");
            RuleFor(d => d.Ubicacion)
                .MaximumLength(200).WithMessage("La ubicación no puede superar los 200 caracteres");
        }
    }
}
