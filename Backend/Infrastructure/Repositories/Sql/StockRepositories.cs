using Application.Repositories;
using Core.Infraestructure.Repositories.Sql;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Sql
{
    internal class CategoriaRepository : BaseRepository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(StoreDbContext context) : base(context) { }
        public async Task<IEnumerable<Categoria>> GetActivasAsync() =>
            await Repository.Where(c => c.Activo).ToListAsync();
    }

    internal class MarcaRepository : BaseRepository<Marca>, IMarcaRepository
    {
        public MarcaRepository(StoreDbContext context) : base(context) { }
        public async Task<IEnumerable<Marca>> GetActivasAsync() =>
            await Repository.Where(m => m.Activo).ToListAsync();
    }

    internal class ProveedorRepository : BaseRepository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(StoreDbContext context) : base(context) { }
        public async Task<IEnumerable<Proveedor>> GetActivosAsync() =>
            await Repository.Where(p => p.Activo).ToListAsync();
        public async Task<Proveedor> GetByCUITAsync(string cuit) =>
            await Repository.FirstOrDefaultAsync(p => p.CUIT == cuit);
    }

    internal class DepositoRepository : BaseRepository<Deposito>, IDepositoRepository
    {
        public DepositoRepository(StoreDbContext context) : base(context) { }
        public async Task<IEnumerable<Deposito>> GetActivosAsync() =>
            await Repository.Where(d => d.Activo).ToListAsync();
    }

    internal class ProductoRepository : BaseRepository<Producto>, IProductoRepository
    {
        public ProductoRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<Producto>> GetActivosAsync() =>
            await Repository.Where(p => p.Activo).ToListAsync();

        public async Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId) =>
            await Repository.Where(p => p.CategoriaId == categoriaId && p.Activo).ToListAsync();

        public async Task<IEnumerable<Producto>> GetStockBajoAsync() =>
            await Repository.Where(p => p.Activo && p.StockActual <= p.StockMinimo).ToListAsync();

        public async Task<Producto> GetByCodigoBarrasAsync(string codigoBarras) =>
            await Repository.FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);

        public async Task<IEnumerable<Producto>> SearchByNombreAsync(string nombre)
        {
            var search = nombre.ToLower();
            return await Repository.Where(p => p.Nombre.ToLower().Contains(search) && p.Activo).ToListAsync();
        }
    }

    internal class MovimientoStockRepository : BaseRepository<MovimientoStock>, IMovimientoStockRepository
    {
        public MovimientoStockRepository(StoreDbContext context) : base(context) { }

        public async Task<IEnumerable<MovimientoStock>> GetByProductoIdAsync(string productoId) =>
            await Repository.Where(m => m.ProductoId == productoId).OrderByDescending(m => m.Fecha).ToListAsync();

        public async Task<IEnumerable<MovimientoStock>> GetByFechaRangoAsync(DateTime desde, DateTime hasta) =>
            await Repository.Where(m => m.Fecha >= desde && m.Fecha <= hasta).OrderByDescending(m => m.Fecha).ToListAsync();
    }
}
