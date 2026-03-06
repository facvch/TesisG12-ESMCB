using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class ConfiguracionSistemaValidator : EntityValidator<ConfiguracionSistema>
    {
        public ConfiguracionSistemaValidator()
        {
            RuleFor(c => c.Clave)
                .NotEmpty().WithMessage("La clave es requerida")
                .MaximumLength(100).WithMessage("No puede superar los 100 caracteres");
            RuleFor(c => c.Valor)
                .NotNull().WithMessage("El valor es requerido");
            RuleFor(c => c.Grupo)
                .NotEmpty().WithMessage("El grupo es requerido")
                .MaximumLength(50).WithMessage("No puede superar los 50 caracteres");
        }
    }
}
