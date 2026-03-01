using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class ServicioValidator : EntityValidator<Servicio>
    {
        public ServicioValidator()
        {
            RuleFor(s => s.Nombre)
                .NotEmpty().WithMessage("El nombre del servicio es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");

            RuleFor(s => s.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres");

            RuleFor(s => s.DuracionMinutos)
                .GreaterThan(0).WithMessage("La duración debe ser mayor a 0 minutos")
                .LessThanOrEqualTo(480).WithMessage("La duración no puede superar las 8 horas");

            RuleFor(s => s.Precio)
                .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo");
        }
    }
}
