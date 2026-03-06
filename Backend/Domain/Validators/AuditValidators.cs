using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class AuditLogValidator : EntityValidator<AuditLog>
    {
        public AuditLogValidator()
        {
            RuleFor(a => a.Accion).NotEmpty().WithMessage("La acción es requerida");
            RuleFor(a => a.Entidad).NotEmpty().WithMessage("La entidad es requerida");
        }
    }

    public class NotificacionValidator : EntityValidator<Notificacion>
    {
        public NotificacionValidator()
        {
            RuleFor(n => n.Titulo)
                .NotEmpty().WithMessage("El título es requerido")
                .MaximumLength(150).WithMessage("No puede superar los 150 caracteres");
            RuleFor(n => n.Mensaje)
                .NotEmpty().WithMessage("El mensaje es requerido")
                .MaximumLength(500).WithMessage("No puede superar los 500 caracteres");
        }
    }
}
