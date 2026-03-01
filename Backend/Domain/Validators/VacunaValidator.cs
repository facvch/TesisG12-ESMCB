using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class VacunaValidator : EntityValidator<Vacuna>
    {
        public VacunaValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre de la vacuna es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");

            RuleFor(v => v.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres");

            RuleFor(v => v.Laboratorio)
                .MaximumLength(100).WithMessage("El laboratorio no puede superar los 100 caracteres");

            RuleFor(v => v.IntervaloDosisDias)
                .GreaterThan(0).When(v => v.IntervaloDosisDias.HasValue)
                .WithMessage("El intervalo entre dosis debe ser mayor a 0 días");
        }
    }
}
