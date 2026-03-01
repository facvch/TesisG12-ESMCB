using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IMetodoPagoRepository : IRepository<MetodoPago>
    {
        Task<IEnumerable<MetodoPago>> GetActivosAsync();
    }

    public interface IVentaRepository : IRepository<Venta>
    {
        Task<IEnumerable<Venta>> GetByPropietarioIdAsync(string propietarioId);
        Task<IEnumerable<Venta>> GetByFechaRangoAsync(DateTime desde, DateTime hasta);
        Task<Venta> GetWithDetallesAsync(string id);
    }

    public interface IDetalleVentaRepository : IRepository<DetalleVenta>
    {
        Task<IEnumerable<DetalleVenta>> GetByVentaIdAsync(string ventaId);
    }

    public interface IFacturaRepository : IRepository<Factura>
    {
        Task<Factura> GetByVentaIdAsync(string ventaId);
        Task<Factura> GetByNumeroAsync(string numero);
    }
}
