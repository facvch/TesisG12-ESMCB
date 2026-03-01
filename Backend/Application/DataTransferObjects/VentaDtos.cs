using Domain.Entities;

namespace Application.DataTransferObjects
{
    public class MetodoPagoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }

    public class VentaDto
    {
        public string Id { get; set; }
        public DateTime Fecha { get; set; }
        public string PropietarioId { get; set; }
        public string PropietarioNombre { get; set; }
        public int MetodoPagoId { get; set; }
        public string MetodoPagoNombre { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }

    public class DetalleVentaDto
    {
        public string Id { get; set; }
        public string ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class FacturaDto
    {
        public string Id { get; set; }
        public string VentaId { get; set; }
        public string Numero { get; set; }
        public string TipoFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal SubTotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
    }
}
