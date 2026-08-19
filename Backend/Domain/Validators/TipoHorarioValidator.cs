using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class TipoHorarioValidator : EntityValidator<TipoHorario>
    {
        public TipoHorarioValidator()
        {
            RuleFor(t => t.Nombre)
                .NotEmpty().WithMessage("El nombre del tipo de horario es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres");

            RuleFor(t => t.Descripcion)
                .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres");
        }
    }
}
