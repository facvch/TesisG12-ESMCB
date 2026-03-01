using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para Dashboard y Reportes agregados
    /// </summary>
    [ApiController]
    public class ReporteController(
        IPacienteRepository pacienteRepo,
        IPropietarioRepository propietarioRepo,
        ITurnoRepository turnoRepo,
        IRegistroVacunacionRepository vacunacionRepo,
        ITratamientoRepository tratamientoRepo,
        IProductoRepository productoRepo,
        IVentaRepository ventaRepo,
        IDetalleVentaRepository detalleVentaRepo,
        IVeterinarioRepository veterinarioRepo,
        IServicioRepository servicioRepo,
        IEspecieRepository especieRepo,
        IMetodoPagoRepository metodoPagoRepo) : BaseController
    {
        // ═══════════════════════════════════════════
        // DASHBOARD
        // ═══════════════════════════════════════════

        /// <summary>
        /// Obtiene las estadísticas generales del dashboard
        /// </summary>
        [HttpGet("api/v1/[Controller]/dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            // Pacientes y propietarios
            var pacientes = await pacienteRepo.FindAllAsync();
            var propietarios = await propietarioRepo.FindAllAsync();

            // Turnos de hoy
            var turnosHoy = await turnoRepo.GetByFechaAsync(hoy);
            var turnosPendientes = turnosHoy.Count(t =>
                t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado);

            // Vacunas pendientes
            var vacunasPendientes = await vacunacionRepo.GetVacunasPendientesAsync();

            // Stock bajo
            var stockBajo = await productoRepo.GetStockBajoAsync();

            // Ventas hoy
            var ventasHoy = await ventaRepo.GetByFechaRangoAsync(hoy, hoy.AddDays(1));
            var ventasConfirmadas = ventasHoy.Where(v => v.Estado == EstadoVenta.Confirmada).ToList();

            // Ventas del mes
            var ventasMes = await ventaRepo.GetByFechaRangoAsync(inicioMes, finMes);
            var ventasMesConf = ventasMes.Where(v => v.Estado == EstadoVenta.Confirmada).ToList();

            // Tratamientos activos (usamos un paciente genérico - buscamos todos)
            var tratamientosActivos = 0;
            foreach (var p in pacientes.Take(100)) // Limitar para performance
            {
                var trats = await tratamientoRepo.GetActivosAsync(p.Id);
                tratamientosActivos += trats.Count();
            }

            return Ok(new DashboardDto
            {
                TotalPacientes = pacientes.Count(),
                TotalPropietarios = propietarios.Count(),
                TurnosHoy = turnosHoy.Count(),
                TurnosPendientes = turnosPendientes,
                VacunasPendientes = vacunasPendientes.Count(),
                ProductosStockBajo = stockBajo.Count(),
                VentasHoy = ventasConfirmadas.Sum(v => v.Total),
                VentasHoyCount = ventasConfirmadas.Count,
                VentasMes = ventasMesConf.Sum(v => v.Total),
                VentasMesCount = ventasMesConf.Count,
                TratamientosActivos = tratamientosActivos
            });
        }

        // ═══════════════════════════════════════════
        // REPORTE DE VENTAS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Reporte de ventas por rango de fechas
        /// </summary>
        [HttpGet("api/v1/[Controller]/ventas")]
        public async Task<IActionResult> GetReporteVentas(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var d = desde ?? DateTime.Today.AddDays(-30);
            var h = hasta ?? DateTime.Today.AddDays(1);

            var ventas = await ventaRepo.GetByFechaRangoAsync(d, h);
            var confirmadas = ventas.Where(v => v.Estado == EstadoVenta.Confirmada).ToList();

            // Ventas por método de pago
            var metodosPago = await metodoPagoRepo.FindAllAsync();
            var ventasPorMetodo = confirmadas
                .GroupBy(v => v.MetodoPagoId)
                .Select(g => new VentaPorMetodoPagoDto
                {
                    MetodoPago = metodosPago.FirstOrDefault(m => m.Id == g.Key)?.Nombre ?? "Desconocido",
                    Cantidad = g.Count(),
                    Total = g.Sum(v => v.Total)
                }).ToList();

            // Ventas por día
            var ventasPorDia = confirmadas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new VentaPorDiaDto
                {
                    Fecha = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(v => v.Total)
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            // Productos más vendidos
            var productosMasVendidos = new List<ProductoMasVendidoDto>();
            foreach (var venta in confirmadas)
            {
                var detalles = await detalleVentaRepo.GetByVentaIdAsync(venta.Id);
                foreach (var det in detalles)
                {
                    var existing = productosMasVendidos.FirstOrDefault(p => p.ProductoId == det.ProductoId);
                    if (existing != null)
                    {
                        existing.CantidadVendida += det.Cantidad;
                        existing.TotalVendido += det.Subtotal;
                    }
                    else
                    {
                        productosMasVendidos.Add(new ProductoMasVendidoDto
                        {
                            ProductoId = det.ProductoId,
                            ProductoNombre = det.Descripcion,
                            CantidadVendida = det.Cantidad,
                            TotalVendido = det.Subtotal
                        });
                    }
                }
            }

            return Ok(new ReporteVentasDto
            {
                Desde = d, Hasta = h,
                CantidadVentas = confirmadas.Count,
                TotalVentas = confirmadas.Sum(v => v.Total),
                PromedioVenta = confirmadas.Any() ? confirmadas.Average(v => v.Total) : 0,
                VentasPorMetodoPago = ventasPorMetodo,
                VentasPorDia = ventasPorDia,
                ProductosMasVendidos = productosMasVendidos.OrderByDescending(p => p.CantidadVendida).Take(10).ToList()
            });
        }

        // ═══════════════════════════════════════════
        // REPORTE DE STOCK
        // ═══════════════════════════════════════════

        /// <summary>
        /// Reporte del estado actual del stock
        /// </summary>
        [HttpGet("api/v1/[Controller]/stock")]
        public async Task<IActionResult> GetReporteStock()
        {
            var todos = await productoRepo.FindAllAsync();
            var activos = todos.Where(p => p.Activo).ToList();
            var stockBajo = activos.Where(p => p.StockBajo).ToList();
            var sinStock = activos.Where(p => p.StockActual == 0).ToList();

            return Ok(new ReporteStockDto
            {
                TotalProductos = todos.Count(),
                ProductosActivos = activos.Count,
                ProductosStockBajo = stockBajo.Count,
                ProductosSinStock = sinStock.Count,
                ValorTotalStock = activos.Sum(p => p.StockActual * p.PrecioCompra),
                ListaStockBajo = stockBajo.Select(p => new ProductoStockBajoDto
                {
                    Id = p.Id, Nombre = p.Nombre,
                    StockActual = p.StockActual, StockMinimo = p.StockMinimo,
                    CategoriaNombre = p.Categoria?.Nombre ?? ""
                }).OrderBy(p => p.StockActual).ToList()
            });
        }

        // ═══════════════════════════════════════════
        // REPORTE DE TURNOS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Reporte de turnos por rango de fechas
        /// </summary>
        [HttpGet("api/v1/[Controller]/turnos")]
        public async Task<IActionResult> GetReporteTurnos(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var d = desde ?? DateTime.Today.AddDays(-30);
            var h = hasta ?? DateTime.Today.AddDays(1);

            // Obtener todos los turnos del período día a día
            var todosTurnos = new List<Turno>();
            for (var dia = d.Date; dia < h.Date; dia = dia.AddDays(1))
            {
                var turnosDia = await turnoRepo.GetByFechaAsync(dia);
                todosTurnos.AddRange(turnosDia);
            }

            var completados = todosTurnos.Count(t => t.Estado == EstadoTurno.Completado);
            var cancelados = todosTurnos.Count(t => t.Estado == EstadoTurno.Cancelado);
            var ausentes = todosTurnos.Count(t => t.Estado == EstadoTurno.Ausente);
            var total = todosTurnos.Count;

            // Turnos por veterinario
            var vets = await veterinarioRepo.FindAllAsync();
            var turnosPorVet = todosTurnos
                .GroupBy(t => t.VeterinarioId)
                .Select(g => new TurnosPorVeterinarioDto
                {
                    VeterinarioId = g.Key,
                    VeterinarioNombre = vets.FirstOrDefault(v => v.Id == g.Key)?.NombreCompleto ?? "Desconocido",
                    TotalTurnos = g.Count(),
                    Completados = g.Count(t => t.Estado == EstadoTurno.Completado)
                }).ToList();

            // Turnos por servicio
            var servicios = await servicioRepo.FindAllAsync();
            var turnosPorServicio = todosTurnos
                .GroupBy(t => t.ServicioId)
                .Select(g => new TurnosPorServicioDto
                {
                    ServicioId = g.Key,
                    ServicioNombre = servicios.FirstOrDefault(s => s.Id == g.Key)?.Nombre ?? "Desconocido",
                    CantidadTurnos = g.Count()
                }).ToList();

            return Ok(new ReporteTurnosDto
            {
                Desde = d, Hasta = h,
                TotalTurnos = total,
                Completados = completados,
                Cancelados = cancelados,
                Ausentes = ausentes,
                TasaCumplimiento = total > 0 ? (decimal)completados / total * 100 : 0,
                TurnosPorVeterinario = turnosPorVet,
                TurnosPorServicio = turnosPorServicio
            });
        }

        // ═══════════════════════════════════════════
        // REPORTE CLÍNICO
        // ═══════════════════════════════════════════

        /// <summary>
        /// Reporte clínico: pacientes por especie, vacunas del mes, tratamientos activos
        /// </summary>
        [HttpGet("api/v1/[Controller]/clinico")]
        public async Task<IActionResult> GetReporteClinico()
        {
            var pacientes = await pacienteRepo.FindAllAsync();
            var especies = await especieRepo.FindAllAsync();

            // Pacientes por especie
            var pacientesPorEspecie = pacientes
                .Where(p => p.Activo)
                .GroupBy(p => p.EspecieId)
                .Select(g => new PacientesPorEspecieDto
                {
                    EspecieId = g.Key,
                    EspecieNombre = especies.FirstOrDefault(e => e.Id == g.Key)?.Nombre ?? "Sin especie",
                    Cantidad = g.Count()
                }).ToList();

            // Vacunas aplicadas este mes
            var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var vacunasAplicadas = 0;
            foreach (var p in pacientes.Where(p => p.Activo).Take(100))
            {
                var registros = await vacunacionRepo.GetByPacienteIdAsync(p.Id);
                vacunasAplicadas += registros.Count(r => r.FechaAplicacion >= inicioMes);
            }

            // Tratamientos activos
            var tratamientosActivos = 0;
            foreach (var p in pacientes.Where(p => p.Activo).Take(100))
            {
                var trats = await tratamientoRepo.GetActivosAsync(p.Id);
                tratamientosActivos += trats.Count();
            }

            return Ok(new ReporteClinicDto
            {
                TotalPacientes = pacientes.Count(p => p.Activo),
                PacientesPorEspecie = pacientesPorEspecie,
                VacunasAplicadasMes = vacunasAplicadas,
                TratamientosActivosCount = tratamientosActivos
            });
        }
    }
}
