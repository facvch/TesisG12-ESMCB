using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class MetodoPagoRepository : BaseRepository<MetodoPago>, IMetodoPagoRepository
    {
        public MetodoPagoRepository(StoreDbContext context) : base(context) { }
        public async Task<IEnumerable<MetodoPago>> GetActivosAsync() =>
            await Repository.Where(m => m.Activo).ToListAsync();
    }

    internal class VentaRepository : BaseRepository<Venta>, IVentaRepository
    {
        public VentaRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Venta>> GetByPropietarioIdAsync(string propietarioId) =>
            await Repository.Where(v => v.PropietarioId == propietarioId)
                .OrderByDescending(v => v.Fecha).ToListAsync();

        public async Task<IEnumerable<Venta>> GetByFechaRangoAsync(DateTime desde, DateTime hasta) =>
            await Repository.Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                .OrderByDescending(v => v.Fecha).ToListAsync();

        public async Task<Venta> GetWithDetallesAsync(string id) =>
            await Repository.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == id);
    }

    internal class DetalleVentaRepository : BaseRepository<DetalleVenta>, IDetalleVentaRepository
    {
        public DetalleVentaRepository(StoreDbContext context) : base(context) { }
        public async Task<IEnumerable<DetalleVenta>> GetByVentaIdAsync(string ventaId) =>
            await Repository.Where(d => d.VentaId == ventaId).ToListAsync();
    }

    internal class FacturaRepository : BaseRepository<Factura>, IFacturaRepository
    {
        public FacturaRepository(StoreDbContext context) : base(context) { }
        public async Task<Factura> GetByVentaIdAsync(string ventaId) =>
            await Repository.FirstOrDefaultAsync(f => f.VentaId == ventaId);
        public async Task<Factura> GetByNumeroAsync(string numero) =>
            await Repository.FirstOrDefaultAsync(f => f.Numero == numero);
    }
}
