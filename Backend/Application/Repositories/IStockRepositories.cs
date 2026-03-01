using Core.Application.Repositories;
using Domain.Entities;

namespace Application.Repositories
{
    public interface ICategoriaRepository : IRepository<Categoria>
    {
        Task<IEnumerable<Categoria>> GetActivasAsync();
    }

    public interface IMarcaRepository : IRepository<Marca>
    {
        Task<IEnumerable<Marca>> GetActivasAsync();
    }

    public interface IProveedorRepository : IRepository<Proveedor>
    {
        Task<IEnumerable<Proveedor>> GetActivosAsync();
        Task<Proveedor> GetByCUITAsync(string cuit);
    }

    public interface IDepositoRepository : IRepository<Deposito>
    {
        Task<IEnumerable<Deposito>> GetActivosAsync();
    }

    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetActivosAsync();
        Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId);
        Task<IEnumerable<Producto>> GetStockBajoAsync();
        Task<Producto> GetByCodigoBarrasAsync(string codigoBarras);
        Task<IEnumerable<Producto>> SearchByNombreAsync(string nombre);
    }

    public interface IMovimientoStockRepository : IRepository<MovimientoStock>
    {
        Task<IEnumerable<MovimientoStock>> GetByProductoIdAsync(string productoId);
        Task<IEnumerable<MovimientoStock>> GetByFechaRangoAsync(DateTime desde, DateTime hasta);
    }
}
