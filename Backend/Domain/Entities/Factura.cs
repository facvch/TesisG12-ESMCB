using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Factura asociada a una venta
    /// </summary>
    public class Factura : DomainEntity<string, FacturaValidator>
    {
        public string VentaId { get; private set; }
        public string Numero { get; private set; }
        public string TipoFactura { get; private set; }  // A, B, C
        public DateTime FechaEmision { get; private set; }
        public decimal SubTotal { get; private set; }
        public decimal IVA { get; private set; }
        public decimal Total { get; private set; }

        // Navegación
        public virtual Venta Venta { get; private set; }

        protected Factura() { }

        public Factura(string ventaId, string numero, string tipoFactura,
            decimal subTotal, decimal iva) : this()
        {
            Id = Guid.NewGuid().ToString();
            VentaId = ventaId;
            Numero = numero;
            TipoFactura = tipoFactura;
            FechaEmision = DateTime.Now;
            SubTotal = subTotal;
            IVA = iva;
            Total = subTotal + iva;
        }
    }
}
