using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Controllers
{
    /// <summary>
    /// Controller para exportar datos en formato CSV
    /// </summary>
    [ApiController]
    public class ExportController(
        IPacienteRepository pacienteRepo,
        IPropietarioRepository propietarioRepo,
        IProductoRepository productoRepo,
        IVentaRepository ventaRepo,
        ITurnoRepository turnoRepo,
        IHistorialClinicoRepository historialRepo,
        IRegistroVacunacionRepository vacunacionRepo,
        ITratamientoRepository tratamientoRepo) : BaseController
    {
        // ═══════════════════════════════
        //  EXPORTAR PACIENTES
        // ═══════════════════════════════

        /// <summary>
        /// Exporta todos los pacientes a CSV
        /// </summary>
        [HttpGet("api/v1/Export/pacientes")]
        public async Task<IActionResult> ExportPacientes()
        {
            var pacientes = await pacienteRepo.FindAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Nombre,Sexo,FechaNacimiento,Especie,Raza,Propietario,PropietarioDNI");

            foreach (var p in pacientes)
            {
                sb.AppendLine(string.Join(",",
                    Escape(p.Id), Escape(p.Nombre), Escape(p.Sexo),
                    p.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "",
                    Escape(p.Especie?.Nombre ?? ""), Escape(p.Raza?.Nombre ?? ""),
                    Escape($"{p.Propietario?.Nombre ?? ""} {p.Propietario?.Apellido ?? ""}"),
                    Escape(p.Propietario?.DNI ?? "")));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"pacientes_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ═══════════════════════════════
        //  EXPORTAR PROPIETARIOS
        // ═══════════════════════════════

        /// <summary>
        /// Exporta todos los propietarios a CSV
        /// </summary>
        [HttpGet("api/v1/Export/propietarios")]
        public async Task<IActionResult> ExportPropietarios()
        {
            var propietarios = await propietarioRepo.FindAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Nombre,Apellido,DNI,Telefono,Email,Direccion");

            foreach (var p in propietarios)
            {
                sb.AppendLine(string.Join(",",
                    Escape(p.Id), Escape(p.Nombre), Escape(p.Apellido),
                    Escape(p.DNI), Escape(p.Telefono),
                    Escape(p.Email ?? ""), Escape(p.Direccion ?? "")));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"propietarios_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ═══════════════════════════════
        //  EXPORTAR HISTORIAL CLÍNICO
        // ═══════════════════════════════

        /// <summary>
        /// Exporta el historial clínico de un paciente a CSV
        /// </summary>
        [HttpGet("api/v1/Export/historial/{pacienteId}")]
        public async Task<IActionResult> ExportHistorial(string pacienteId)
        {
            var paciente = await pacienteRepo.FindOneAsync(pacienteId);
            if (paciente == null) return NotFound("Paciente no encontrado");

            var historial = await historialRepo.GetByPacienteIdAsync(pacienteId);
            var vacunaciones = await vacunacionRepo.GetByPacienteIdAsync(pacienteId);
            var tratamientos = await tratamientoRepo.GetByPacienteIdAsync(pacienteId);

            var sb = new StringBuilder();
            sb.AppendLine($"HISTORIAL CLÍNICO - {paciente.Nombre}");
            sb.AppendLine($"Especie: {paciente.Especie?.Nombre ?? ""} | Propietario: {paciente.Propietario?.Nombre ?? ""} {paciente.Propietario?.Apellido ?? ""}");
            sb.AppendLine();

            // Consultas
            sb.AppendLine("=== CONSULTAS ===");
            sb.AppendLine("Fecha,Motivo,Veterinario,Sintomas,Diagnostico,Indicaciones,Peso,Temperatura,Observaciones");
            foreach (var h in historial.OrderByDescending(x => x.Fecha))
            {
                sb.AppendLine(string.Join(",",
                    h.Fecha.ToString("dd/MM/yyyy"), Escape(h.Motivo), Escape(h.Veterinario),
                    Escape(h.Sintomas), Escape(h.Diagnostico), Escape(h.Indicaciones),
                    h.Peso?.ToString() ?? "", h.Temperatura?.ToString() ?? "", Escape(h.Observaciones)));
            }

            sb.AppendLine();

            // Vacunaciones
            sb.AppendLine("=== VACUNACIONES ===");
            sb.AppendLine("FechaAplicacion,Vacuna,Veterinario,NroLote,ProximaDosis,Observaciones");
            foreach (var v in vacunaciones.OrderByDescending(x => x.FechaAplicacion))
            {
                sb.AppendLine(string.Join(",",
                    v.FechaAplicacion.ToString("dd/MM/yyyy"), Escape(v.Vacuna?.Nombre ?? ""),
                    Escape(v.Veterinario), Escape(v.NroLote),
                    v.FechaProximaDosis?.ToString("dd/MM/yyyy") ?? "", Escape(v.Observaciones)));
            }

            sb.AppendLine();

            // Tratamientos
            sb.AppendLine("=== TRATAMIENTOS ===");
            sb.AppendLine("Fecha,Diagnostico,Descripcion,Medicacion,Veterinario,Finalizado,Observaciones");
            foreach (var t in tratamientos.OrderByDescending(x => x.Fecha))
            {
                sb.AppendLine(string.Join(",",
                    t.Fecha.ToString("dd/MM/yyyy"), Escape(t.Diagnostico), Escape(t.Descripcion),
                    Escape(t.Medicacion), Escape(t.Veterinario),
                    t.Finalizado ? "Sí" : "No", Escape(t.Observaciones)));
            }

            var fileName = $"historial_{paciente.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }

        // ═══════════════════════════════
        //  EXPORTAR PRODUCTOS / STOCK
        // ═══════════════════════════════

        /// <summary>
        /// Exporta el inventario completo a CSV
        /// </summary>
        [HttpGet("api/v1/Export/productos")]
        public async Task<IActionResult> ExportProductos([FromQuery] bool soloActivos = true)
        {
            var productos = await productoRepo.FindAllAsync();
            if (soloActivos) productos = productos.Where(p => p.Activo).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Id,Nombre,CodigoBarras,Categoria,Marca,Proveedor,Deposito,PrecioCompra,PrecioVenta,StockActual,StockMinimo,Activo");

            foreach (var p in productos.OrderBy(p => p.Nombre))
            {
                sb.AppendLine(string.Join(",",
                    p.Id, Escape(p.Nombre), Escape(p.CodigoBarras ?? ""),
                    Escape(p.Categoria?.Nombre ?? ""), Escape(p.Marca?.Nombre ?? ""),
                    Escape(p.Proveedor?.RazonSocial ?? ""), Escape(p.Deposito?.Nombre ?? ""),
                    p.PrecioCompra, p.PrecioVenta, p.StockActual, p.StockMinimo,
                    p.Activo ? "Sí" : "No"));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"productos_{DateTime.Now:yyyyMMdd}.csv");
        }

        /// <summary>
        /// Exporta solo productos con stock bajo a CSV
        /// </summary>
        [HttpGet("api/v1/Export/stockBajo")]
        public async Task<IActionResult> ExportStockBajo()
        {
            var productos = (await productoRepo.FindAllAsync())
                .Where(p => p.Activo && p.StockActual <= p.StockMinimo).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"ALERTA STOCK BAJO - {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Productos con stock bajo: {productos.Count}");
            sb.AppendLine();
            sb.AppendLine("Nombre,StockActual,StockMinimo,Faltante,Proveedor,Telefono Proveedor");

            foreach (var p in productos.OrderBy(p => p.StockActual))
            {
                sb.AppendLine(string.Join(",",
                    Escape(p.Nombre), p.StockActual, p.StockMinimo,
                    p.StockMinimo - p.StockActual,
                    Escape(p.Proveedor?.RazonSocial ?? ""),
                    Escape(p.Proveedor?.Telefono ?? "")));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"stock_bajo_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ═══════════════════════════════
        //  EXPORTAR VENTAS
        // ═══════════════════════════════

        /// <summary>
        /// Exporta ventas en un rango de fechas a CSV
        /// </summary>
        [HttpGet("api/v1/Export/ventas")]
        public async Task<IActionResult> ExportVentas(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var d = desde ?? DateTime.Today.AddMonths(-1);
            var h = hasta ?? DateTime.Today.AddDays(1);
            var ventas = await ventaRepo.GetByFechaRangoAsync(d, h);

            var sb = new StringBuilder();
            sb.AppendLine($"REPORTE DE VENTAS - {d:dd/MM/yyyy} al {h:dd/MM/yyyy}");
            sb.AppendLine($"Total ventas: {ventas.Count()} | Monto total: ${ventas.Where(v => v.Estado != Domain.Entities.EstadoVenta.Anulada).Sum(v => v.Total)}");
            sb.AppendLine();
            sb.AppendLine("Id,Fecha,Propietario,Total,MetodoPago,Anulada,CantidadItems");

            foreach (var v in ventas.OrderByDescending(x => x.Fecha))
            {
                sb.AppendLine(string.Join(",",
                    Escape(v.Id), v.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    Escape($"{v.Propietario?.Nombre ?? ""} {v.Propietario?.Apellido ?? ""}"),
                    v.Total, Escape(v.MetodoPago?.Nombre ?? ""),
                    v.Estado == Domain.Entities.EstadoVenta.Anulada ? "ANULADA" : v.Estado.ToString(),
                    v.Detalles?.Count ?? 0));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"ventas_{d:yyyyMMdd}_{h:yyyyMMdd}.csv");
        }

        // ═══════════════════════════════
        //  EXPORTAR TURNOS / AGENDA
        // ═══════════════════════════════

        /// <summary>
        /// Exporta la agenda de turnos a CSV
        /// </summary>
        [HttpGet("api/v1/Export/turnos")]
        public async Task<IActionResult> ExportTurnos(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var d = desde ?? DateTime.Today;
            var h = hasta ?? DateTime.Today.AddDays(7);
            var turnos = (await turnoRepo.FindAllAsync())
                .Where(t => t.FechaHora >= d && t.FechaHora <= h)
                .OrderBy(t => t.FechaHora).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"AGENDA DE TURNOS - {d:dd/MM/yyyy} al {h:dd/MM/yyyy}");
            sb.AppendLine($"Total turnos: {turnos.Count}");
            sb.AppendLine();
            sb.AppendLine("Fecha,Hora,Paciente,Propietario,Veterinario,Servicio,Estado,Motivo,Duracion(min)");

            foreach (var t in turnos)
            {
                sb.AppendLine(string.Join(",",
                    t.FechaHora.ToString("dd/MM/yyyy"), t.FechaHora.ToString("HH:mm"),
                    Escape(t.Paciente?.Nombre ?? ""),
                    Escape(t.Paciente?.Propietario != null ? $"{t.Paciente.Propietario.Nombre} {t.Paciente.Propietario.Apellido}" : ""),
                    Escape(t.Veterinario != null ? $"Dr. {t.Veterinario.Apellido}" : ""),
                    Escape(t.Servicio?.Nombre ?? ""),
                    t.Estado.ToString(), Escape(t.Motivo), t.DuracionMinutos));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"turnos_{d:yyyyMMdd}_{h:yyyyMMdd}.csv");
        }

        // ═══════════════════════════════

        /// <summary>
        /// Escapa un valor para CSV (envuelve en comillas si contiene coma o salto de línea)
        /// </summary>
        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
