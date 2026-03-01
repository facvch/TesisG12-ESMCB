using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    public class MovimientoStock : DomainEntity<string, MovimientoStockValidator>
    {
        public string ProductoId { get; private set; }
        public TipoMovimiento Tipo { get; private set; }
        public int Cantidad { get; private set; }
        public DateTime Fecha { get; private set; }
        public string Motivo { get; private set; }
        public string Referencia { get; private set; } // Nro factura, remito, etc.

        // Navegación
        public virtual Producto Producto { get; private set; }

        protected MovimientoStock() { }

        public MovimientoStock(string productoId, TipoMovimiento tipo, int cantidad,
            string motivo = "", string referencia = "") : this()
        {
            Id = Guid.NewGuid().ToString();
            ProductoId = productoId;
            Tipo = tipo;
            Cantidad = cantidad;
            Fecha = DateTime.Now;
            Motivo = motivo;
            Referencia = referencia;
        }
    }

    public enum TipoMovimiento
    {
        Entrada = 0,
        Salida = 1,
        Ajuste = 2,
        Devolucion = 3
    }
}
