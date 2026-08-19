using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Validators
{
    public class VeterinarioValidator : EntityValidator<Veterinario>
    {
        private static readonly List<string> EspecialidadesValidas = new()
        {
            "Clínica General",
            "Cirugía",
            "Cirugía General",
            "Cirugía Plástica",
            "Dermatología",
            "Traumatología",
            "Cardiología",
            "Oftalmología",
            "Neurología",
            "Oncología",
            "Anestesiología",
            "Fisioterapia",
            "Pediatría",
            "Obstetricia",
            "Nutrición"
        };

        public VeterinarioValidator()
        {
            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres");

            RuleFor(v => v.Apellido)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MaximumLength(50).WithMessage("El apellido no puede superar los 50 caracteres");

            RuleFor(v => v.Matricula)
                .NotEmpty().WithMessage("Debe ingresar la matrícula profesional del veterinario")
                .MaximumLength(20).WithMessage("La matrícula no puede superar los 20 caracteres")
                .Must(m => m == null || m.Count(char.IsDigit) >= 3).WithMessage("La cantidad de dígitos ingresados es menor a la válida (3 a 5)")
                .Must(m => m == null || m.Count(char.IsDigit) <= 5).WithMessage("La cantidad de dígitos ingresados es mayor a la válida (3 a 5)");

            RuleFor(v => v.Telefono)
                .NotEmpty().WithMessage("Debe ingresar un número de teléfono válido")
                .MaximumLength(20).WithMessage("El teléfono no puede superar los 20 caracteres")
                .Must(BeAValidArgentinePhone).WithMessage("El número ingresado debe ser válido en Argentina");

            RuleFor(v => v.Email)
                .MaximumLength(100).WithMessage("El email no puede superar los 100 caracteres")
                .Must(BeAValidEmail).When(v => !string.IsNullOrEmpty(v.Email))
                .WithMessage("El formato del email no es válido");

            RuleFor(v => v.Especialidad)
                .NotEmpty().WithMessage("Debe elegir una especialidad")
                .MaximumLength(100).WithMessage("La especialidad no puede superar los 100 caracteres")
                .Must(BeAValidSpecialty).WithMessage("Debe elegir una especialidad");
        }

        private bool BeAValidArgentinePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            string clean = System.Text.RegularExpressions.Regex.Replace(phone, @"[() \-\.]", "");
            var regex = new System.Text.RegularExpressions.Regex(@"^(?:0?[1-9]\d{1,3})(?:15)?\d{6,8}$");
            return regex.IsMatch(clean);
        }

        private bool BeAValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return true;
            var regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }

        private bool BeAValidSpecialty(string especialidad)
        {
            if (string.IsNullOrWhiteSpace(especialidad)) return false;
            return EspecialidadesValidas.Any(e => e.Equals(especialidad, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
