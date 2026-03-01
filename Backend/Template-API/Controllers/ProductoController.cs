using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestionar Productos con control de stock
    /// </summary>
    [ApiController]
    public class ProductoController(
        IProductoRepository productoRepo,
        ICategoriaRepository categoriaRepo,
        IMovimientoStockRepository movimientoRepo) : BaseController
    {
        private readonly IProductoRepository _productoRepo = productoRepo ?? throw new ArgumentNullException(nameof(productoRepo));
        private readonly ICategoriaRepository _categoriaRepo = categoriaRepo ?? throw new ArgumentNullException(nameof(categoriaRepo));
        private readonly IMovimientoStockRepository _movimientoRepo = movimientoRepo ?? throw new ArgumentNullException(nameof(movimientoRepo));

        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll(bool soloActivos = true)
        {
            var entities = soloActivos ? await _productoRepo.GetActivosAsync() : await _productoRepo.FindAllAsync();
            var dtos = entities.Select(MapToDto).ToList();
            return Ok(new QueryResult<ProductoDto>(dtos, dtos.Count, 1, 10));
        }

        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var entity = await _productoRepo.FindOneAsync(id);
            if (entity == null) return NotFound();
            return Ok(MapToDto(entity));
        }

        [HttpGet("api/v1/[Controller]/search")]
        public async Task<IActionResult> Search([FromQuery] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return BadRequest("Debe proporcionar un término de búsqueda");
            var entities = await _productoRepo.SearchByNombreAsync(nombre);
            return Ok(entities.Select(MapToDto).ToList());
        }

        [HttpGet("api/v1/[Controller]/byCategoria/{categoriaId}")]
        public async Task<IActionResult> GetByCategoria(int categoriaId)
        {
            var entities = await _productoRepo.GetByCategoriaIdAsync(categoriaId);
            return Ok(entities.Select(MapToDto).ToList());
        }

        [HttpGet("api/v1/[Controller]/byCodigoBarras/{codigo}")]
        public async Task<IActionResult> GetByCodigoBarras(string codigo)
        {
            var entity = await _productoRepo.GetByCodigoBarrasAsync(codigo);
            if (entity == null) return NotFound();
            return Ok(MapToDto(entity));
        }

        /// <summary>
        /// Obtiene productos con stock bajo (stock actual <= stock mínimo)
        /// </summary>
        [HttpGet("api/v1/[Controller]/stockBajo")]
        public async Task<IActionResult> GetStockBajo()
        {
            var entities = await _productoRepo.GetStockBajoAsync();
            return Ok(entities.Select(MapToDto).ToList());
        }

        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateProductoRequest r)
        {
            if (r is null) return BadRequest("El request no puede ser nulo");

            var categoria = await _categoriaRepo.FindOneAsync(r.CategoriaId);
            if (categoria == null) return BadRequest($"No existe la categoría con Id {r.CategoriaId}");

            var entity = new Domain.Entities.Producto(
                r.Nombre, r.Descripcion ?? "", r.CodigoBarras ?? "",
                r.CategoriaId, r.PrecioCompra, r.PrecioVenta,
                r.StockActual, r.StockMinimo,
                r.MarcaId, r.ProveedorId, r.DepositoId);

            if (!entity.IsValid) return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));
            var id = await _productoRepo.AddAsync(entity);
            return Created($"api/v1/Producto/{id}", new { Id = id });
        }

        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateProductoRequest r)
        {
            var entity = await _productoRepo.FindOneAsync(r.Id);
            if (entity == null) return NotFound();
            entity.Actualizar(r.Nombre, r.Descripcion ?? "", r.PrecioCompra, r.PrecioVenta, r.StockMinimo);
            if (!entity.IsValid) return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));
            _productoRepo.Update(r.Id, entity);
            return NoContent();
        }

        /// <summary>
        /// Registra entrada de stock
        /// </summary>
        [HttpPost("api/v1/[Controller]/{id}/entrada")]
        public async Task<IActionResult> EntradaStock(string id, [FromBody] MovimientoStockRequest r)
        {
            var entity = await _productoRepo.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el producto con Id {id}");
            if (r.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a 0");

            entity.AgregarStock(r.Cantidad);
            _productoRepo.Update(id, entity);

            var movimiento = new Domain.Entities.MovimientoStock(
                id, TipoMovimiento.Entrada, r.Cantidad, r.Motivo ?? "Entrada de stock", r.Referencia ?? "");
            await _movimientoRepo.AddAsync(movimiento);

            return Ok(new { StockActual = entity.StockActual });
        }

        /// <summary>
        /// Registra salida de stock
        /// </summary>
        [HttpPost("api/v1/[Controller]/{id}/salida")]
        public async Task<IActionResult> SalidaStock(string id, [FromBody] MovimientoStockRequest r)
        {
            var entity = await _productoRepo.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el producto con Id {id}");
            if (r.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a 0");

            if (!entity.DescontarStock(r.Cantidad))
                return BadRequest($"Stock insuficiente. Stock actual: {entity.StockActual}, cantidad solicitada: {r.Cantidad}");

            _productoRepo.Update(id, entity);

            var movimiento = new Domain.Entities.MovimientoStock(
                id, TipoMovimiento.Salida, r.Cantidad, r.Motivo ?? "Salida de stock", r.Referencia ?? "");
            await _movimientoRepo.AddAsync(movimiento);

            return Ok(new { StockActual = entity.StockActual });
        }

        /// <summary>
        /// Obtiene el historial de movimientos de un producto
        /// </summary>
        [HttpGet("api/v1/[Controller]/{id}/movimientos")]
        public async Task<IActionResult> GetMovimientos(string id)
        {
            var movimientos = await _movimientoRepo.GetByProductoIdAsync(id);
            var dtos = movimientos.Select(m => new MovimientoStockDto
            {
                Id = m.Id, ProductoId = m.ProductoId,
                ProductoNombre = m.Producto?.Nombre ?? "",
                Tipo = m.Tipo.ToString(), Cantidad = m.Cantidad,
                Fecha = m.Fecha, Motivo = m.Motivo, Referencia = m.Referencia
            }).ToList();
            return Ok(dtos);
        }

        [HttpDelete("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _productoRepo.FindOneAsync(id);
            if (entity == null) return NotFound();
            entity.Desactivar();
            _productoRepo.Update(id, entity);
            return NoContent();
        }

        private static ProductoDto MapToDto(Domain.Entities.Producto p) => new()
        {
            Id = p.Id, Nombre = p.Nombre, Descripcion = p.Descripcion,
            CodigoBarras = p.CodigoBarras, CategoriaId = p.CategoriaId,
            CategoriaNombre = p.Categoria?.Nombre ?? "",
            MarcaId = p.MarcaId, MarcaNombre = p.Marca?.Nombre ?? "",
            ProveedorId = p.ProveedorId, ProveedorNombre = p.Proveedor?.RazonSocial ?? "",
            DepositoId = p.DepositoId, DepositoNombre = p.Deposito?.Nombre ?? "",
            PrecioCompra = p.PrecioCompra, PrecioVenta = p.PrecioVenta,
            StockActual = p.StockActual, StockMinimo = p.StockMinimo,
            StockBajo = p.StockBajo, Activo = p.Activo
        };
    }

    public class CreateProductoRequest
    {
        public string Nombre { get; set; } public string Descripcion { get; set; }
        public string CodigoBarras { get; set; } public int CategoriaId { get; set; }
        public int? MarcaId { get; set; } public string ProveedorId { get; set; }
        public int? DepositoId { get; set; } public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; } public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }

    public class UpdateProductoRequest
    {
        public string Id { get; set; } public string Nombre { get; set; }
        public string Descripcion { get; set; } public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; } public int StockMinimo { get; set; }
    }

    public class MovimientoStockRequest
    {
        public int Cantidad { get; set; }
        public string Motivo { get; set; }
        public string Referencia { get; set; }
    }
}
