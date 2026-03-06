using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class RolValidator : EntityValidator<Rol>
    {
        public RolValidator()
        {
            RuleFor(r => r.Nombre)
                .NotEmpty().WithMessage("El nombre del rol es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres");
        }
    }

    public class UsuarioValidator : EntityValidator<Usuario>
    {
        public UsuarioValidator()
        {
            RuleFor(u => u.NombreUsuario)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El nombre de usuario no puede superar los 50 caracteres");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no es válido")
                .MaximumLength(100).WithMessage("El email no puede superar los 100 caracteres");

            RuleFor(u => u.NombreCompleto)
                .NotEmpty().WithMessage("El nombre completo es requerido")
                .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres");

            RuleFor(u => u.RolId)
                .GreaterThan(0).WithMessage("Debe asignar un rol al usuario");
        }
    }
}
