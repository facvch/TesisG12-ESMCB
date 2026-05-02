using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    public class MetodoPagoController(IMetodoPagoRepository repo) : BaseController
    {
        private readonly IMetodoPagoRepository _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll(bool soloActivos = true)
        {
            var entities = soloActivos ? await _repo.GetActivosAsync() : await _repo.FindAllAsync();
            var dtos = entities.Select(m => new MetodoPagoDto { Id = m.Id, Nombre = m.Nombre, Activo = m.Activo }).ToList();
            return Ok(new QueryResult<MetodoPagoDto>(dtos, dtos.Count, 1, 10));
        }

        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _repo.FindOneAsync(id);
            if (e == null) return NotFound();
            return Ok(new MetodoPagoDto { Id = e.Id, Nombre = e.Nombre, Activo = e.Activo });
        }

        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateMetodoPagoRequest r)
        {
            var entity = new Domain.Entities.MetodoPago(r.Nombre);
            if (!entity.IsValid) return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));
            var id = await _repo.AddAsync(entity);
            return Created($"api/v1/MetodoPago/{id}", new { Id = id });
        }

        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateMetodoPagoRequest r)
        {
            var e = await _repo.FindOneAsync(r.Id);
            if (e == null) return NotFound();
            e.Actualizar(r.Nombre);
            _repo.Update(r.Id, e);
            return NoContent();
        }

        [HttpDelete("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _repo.FindOneAsync(id);
            if (e == null) return NotFound();
            e.Desactivar(); _repo.Update(id, e);
            return NoContent();
        }
    }

    public class CreateMetodoPagoRequest { public string Nombre { get; set; } }
    public class UpdateMetodoPagoRequest { public int Id { get; set; } public string Nombre { get; set; } }

    /// <summary>
    /// Controller para gestionar Ventas con descuento automático de stock
    /// </summary>
    [ApiController]
    public class VentaController(
        IVentaRepository ventaRepo,
        IDetalleVentaRepository detalleRepo,
        IPropietarioRepository propietarioRepo,
        IMetodoPagoRepository metodoPagoRepo,
        IProductoRepository productoRepo,
        IMovimientoStockRepository movimientoRepo,
        IFacturaRepository facturaRepo) : BaseController
    {
        private readonly IVentaRepository _ventaRepo = ventaRepo;
        private readonly IDetalleVentaRepository _detalleRepo = detalleRepo;
        private readonly IPropietarioRepository _propietarioRepo = propietarioRepo;
        private readonly IMetodoPagoRepository _metodoPagoRepo = metodoPagoRepo;
        private readonly IProductoRepository _productoRepo = productoRepo;
        private readonly IMovimientoStockRepository _movimientoRepo = movimientoRepo;
        private readonly IFacturaRepository _facturaRepo = facturaRepo;

        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetByFecha([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var d = desde ?? DateTime.Today;
            var h = hasta ?? DateTime.Today.AddDays(1);
            var entities = await _ventaRepo.GetByFechaRangoAsync(d, h);
            return Ok(entities.Select(MapToDto).ToList());
        }

        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var entity = await _ventaRepo.GetWithDetallesAsync(id);
            if (entity == null) return NotFound();
            return Ok(MapToDto(entity));
        }

        [HttpGet("api/v1/[Controller]/byPropietario/{propietarioId}")]
        public async Task<IActionResult> GetByPropietario(string propietarioId)
        {
            var entities = await _ventaRepo.GetByPropietarioIdAsync(propietarioId);
            return Ok(entities.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Crea una venta con sus detalles y descuenta stock automáticamente
        /// </summary>
        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateVentaRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");
            if (request.Detalles == null || !request.Detalles.Any())
                return BadRequest("La venta debe tener al menos un detalle");

            // Validar propietario (opcional)
            if (!string.IsNullOrWhiteSpace(request.PropietarioId))
            {
                var prop = await _propietarioRepo.FindOneAsync(request.PropietarioId);
                if (prop == null) return BadRequest($"No existe el propietario con Id {request.PropietarioId}");
            }

            // Validar método de pago
            var metodo = await _metodoPagoRepo.FindOneAsync(request.MetodoPagoId);
            if (metodo == null) return BadRequest($"No existe el método de pago con Id {request.MetodoPagoId}");

            // Crear la venta
            var propId = string.IsNullOrWhiteSpace(request.PropietarioId) ? null : request.PropietarioId;
            var venta = new Domain.Entities.Venta(
                propId, request.MetodoPagoId, request.Observaciones ?? "");

            if (!venta.IsValid) return BadRequest(venta.GetErrors().Select(e => e.ErrorMessage));

            var ventaId = (await _ventaRepo.AddAsync(venta)).ToString();

            // Procesar cada detalle
            decimal totalVenta = 0;
            foreach (var det in request.Detalles)
            {
                // Validar producto y stock
                var producto = await _productoRepo.FindOneAsync(det.ProductoId);
                if (producto == null)
                    return BadRequest($"No existe el producto con Id {det.ProductoId}");

                if (!producto.DescontarStock(det.Cantidad))
                    return BadRequest($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.StockActual}");

                _productoRepo.Update(det.ProductoId, producto);

                // Registrar movimiento de stock
                var movimiento = new Domain.Entities.MovimientoStock(
                    det.ProductoId, TipoMovimiento.Salida, det.Cantidad, "Venta", ventaId.ToString());
                await _movimientoRepo.AddAsync(movimiento);

                // Crear detalle y guardar en DB
                var precioUnit = det.PrecioUnitario > 0 ? det.PrecioUnitario : producto.PrecioVenta;
                var detalle = new Domain.Entities.DetalleVenta(
                    ventaId, det.ProductoId, det.Descripcion ?? producto.Nombre,
                    det.Cantidad, precioUnit);

                await _detalleRepo.AddAsync(detalle);
                totalVenta += detalle.Subtotal;
            }

            // Actualizar total y confirmar
            venta.ActualizarTotal(totalVenta);
            venta.Confirmar();
            _ventaRepo.Update(ventaId, venta);

            return Created($"api/v1/Venta/{ventaId}", new { Id = ventaId, Total = venta.Total });
        }

        /// <summary>
        /// Anula una venta y revierte el stock
        /// </summary>
        [HttpPut("api/v1/[Controller]/{id}/anular")]
        public async Task<IActionResult> Anular(string id, [FromBody] string motivo = "")
        {
            var venta = await _ventaRepo.GetWithDetallesAsync(id);
            if (venta == null) return NotFound();
            if (venta.Estado == EstadoVenta.Anulada) return BadRequest("La venta ya está anulada");

            // Revertir stock
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _productoRepo.FindOneAsync(detalle.ProductoId);
                if (producto != null)
                {
                    producto.AgregarStock(detalle.Cantidad);
                    _productoRepo.Update(detalle.ProductoId, producto);

                    var movimiento = new Domain.Entities.MovimientoStock(
                        detalle.ProductoId, TipoMovimiento.Devolucion, detalle.Cantidad,
                        $"Anulación venta {id}", "");
                    await _movimientoRepo.AddAsync(movimiento);
                }
            }

            venta.Anular(motivo);
            _ventaRepo.Update(id, venta);
            return NoContent();
        }

        /// <summary>
        /// Genera una factura para una venta
        /// </summary>
        [HttpPost("api/v1/[Controller]/{id}/facturar")]
        public async Task<IActionResult> Facturar(string id, [FromBody] FacturarRequest request)
        {
            var venta = await _ventaRepo.GetWithDetallesAsync(id);
            if (venta == null) return NotFound();
            if (venta.Estado == EstadoVenta.Anulada) return BadRequest("No se puede facturar una venta anulada");

            var existingFactura = await _facturaRepo.GetByVentaIdAsync(id);
            if (existingFactura != null) return BadRequest("Esta venta ya tiene una factura asociada");

            var iva = venta.Total * (request.PorcentajeIVA / 100m);
            var factura = new Domain.Entities.Factura(
                id, request.Numero, request.TipoFactura, venta.Total, iva);

            if (!factura.IsValid) return BadRequest(factura.GetErrors().Select(e => e.ErrorMessage));

            var facturaId = (await _facturaRepo.AddAsync(factura)).ToString();
            return Created($"api/v1/Factura/{facturaId}", new FacturaDto
            {
                Id = facturaId, VentaId = id, Numero = factura.Numero,
                TipoFactura = factura.TipoFactura, FechaEmision = factura.FechaEmision,
                SubTotal = factura.SubTotal, IVA = factura.IVA, Total = factura.Total
            });
        }

        /// <summary>
        /// Obtiene una factura por número
        /// </summary>
        [HttpGet("api/v1/Factura/byNumero/{numero}")]
        public async Task<IActionResult> GetFacturaByNumero(string numero)
        {
            var f = await _facturaRepo.GetByNumeroAsync(numero);
            if (f == null) return NotFound();
            return Ok(new FacturaDto
            {
                Id = f.Id, VentaId = f.VentaId, Numero = f.Numero,
                TipoFactura = f.TipoFactura, FechaEmision = f.FechaEmision,
                SubTotal = f.SubTotal, IVA = f.IVA, Total = f.Total
            });
        }

        private static VentaDto MapToDto(Domain.Entities.Venta v) => new()
        {
            Id = v.Id, Fecha = v.Fecha,
            PropietarioId = v.PropietarioId,
            PropietarioNombre = v.Propietario?.NombreCompleto ?? "",
            MetodoPagoId = v.MetodoPagoId,
            MetodoPagoNombre = v.MetodoPago?.Nombre ?? "",
            Total = v.Total, Estado = v.Estado.ToString(),
            Observaciones = v.Observaciones,
            Detalles = v.Detalles?.Select(d => new DetalleVentaDto
            {
                Id = d.Id, ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? "",
                Descripcion = d.Descripcion, Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario, Subtotal = d.Subtotal
            }).ToList() ?? new()
        };
    }

    public class CreateVentaRequest
    {
        public string PropietarioId { get; set; }
        public int MetodoPagoId { get; set; }
        public string Observaciones { get; set; }
        public List<CreateDetalleVentaRequest> Detalles { get; set; }
    }

    public class CreateDetalleVentaRequest
    {
        public string ProductoId { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class FacturarRequest
    {
        public string Numero { get; set; }
        public string TipoFactura { get; set; }
        public decimal PorcentajeIVA { get; set; } = 21;
    }
}
