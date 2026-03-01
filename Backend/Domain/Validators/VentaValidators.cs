using Core.Domain.Validators;
using Domain.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class MetodoPagoValidator : EntityValidator<MetodoPago>
    {
        public MetodoPagoValidator()
        {
            RuleFor(m => m.Nombre)
                .NotEmpty().WithMessage("El nombre del método de pago es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres");
        }
    }

    public class DetalleVentaValidator : EntityValidator<DetalleVenta>
    {
        public DetalleVentaValidator()
        {
            RuleFor(d => d.VentaId).NotEmpty().WithMessage("Debe indicar la venta");
            RuleFor(d => d.Descripcion)
                .NotEmpty().WithMessage("La descripción es requerida")
                .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres");
            RuleFor(d => d.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");
            RuleFor(d => d.PrecioUnitario).GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo");
        }
    }

    public class VentaValidator : EntityValidator<Venta>
    {
        public VentaValidator()
        {
            RuleFor(v => v.MetodoPagoId).GreaterThan(0).WithMessage("Debe seleccionar un método de pago");
        }
    }

    public class FacturaValidator : EntityValidator<Factura>
    {
        public FacturaValidator()
        {
            RuleFor(f => f.VentaId).NotEmpty().WithMessage("Debe indicar la venta");
            RuleFor(f => f.Numero)
                .NotEmpty().WithMessage("El número de factura es requerido")
                .MaximumLength(20).WithMessage("El número no puede superar los 20 caracteres");
            RuleFor(f => f.TipoFactura)
                .NotEmpty().WithMessage("El tipo de factura es requerido")
                .Must(t => t == "A" || t == "B" || t == "C")
                .WithMessage("El tipo de factura debe ser A, B o C");
            RuleFor(f => f.SubTotal).GreaterThanOrEqualTo(0).WithMessage("El subtotal no puede ser negativo");
            RuleFor(f => f.IVA).GreaterThanOrEqualTo(0).WithMessage("El IVA no puede ser negativo");
        }
    }
}
