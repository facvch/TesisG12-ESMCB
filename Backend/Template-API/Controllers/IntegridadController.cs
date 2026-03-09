using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller de integridad del sistema, health check y validaciones
    /// </summary>
    [ApiController]
    public class IntegridadController(
        IPacienteRepository pacienteRepo,
        IPropietarioRepository propietarioRepo,
        IProductoRepository productoRepo,
        IEspecieRepository especieRepo,
        IRazaRepository razaRepo,
        IVeterinarioRepository veterinarioRepo,
        ITurnoRepository turnoRepo,
        IVentaRepository ventaRepo,
        IServicioRepository servicioRepo) : BaseController
    {
        // ═══════════════════════════════
        //  HEALTH CHECK
        // ═══════════════════════════════

        /// <summary>
        /// Health check del sistema - verifica DB y servicios
        /// </summary>
        [HttpGet("api/v1/health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // Verificar acceso a DB consultando entidades básicas
                var especies = await especieRepo.FindAllAsync();
                var propietarios = await propietarioRepo.FindAllAsync();

                return Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.Now,
                    Database = "Connected",
                    Version = "1.0.0",
                    Entidades = new
                    {
                        Especies = especies.Count,
                        Propietarios = propietarios.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.Now,
                    Error = ex.Message
                });
            }
        }

        // ═══════════════════════════════
        //  VALIDAR DEPENDENCIAS ANTES DE ELIMINAR
        // ═══════════════════════════════

        /// <summary>
        /// Verifica si una especie puede ser eliminada (no tiene pacientes activos)
        /// </summary>
        [HttpGet("api/v1/Integridad/especie/{id}/dependencias")]
        public async Task<IActionResult> DependenciasEspecie(int id)
        {
            var especie = await especieRepo.FindOneAsync(id);
            if (especie == null) return NotFound();

            var pacientes = (await pacienteRepo.FindAllAsync())
                .Where(p => p.EspecieId == id).ToList();
            var razas = (await razaRepo.FindAllAsync())
                .Where(r => r.EspecieId == id).ToList();

            return Ok(new DependencyCheckResult
            {
                EntidadId = id.ToString(),
                EntidadTipo = "Especie",
                EntidadNombre = especie.Nombre,
                PuedeEliminar = !pacientes.Any() && !razas.Any(),
                Dependencias = new List<DependencyDetail>
                {
                    new() { Tipo = "Pacientes", Cantidad = pacientes.Count,
                        Items = pacientes.Take(5).Select(p => p.Nombre).ToList() },
                    new() { Tipo = "Razas", Cantidad = razas.Count,
                        Items = razas.Take(5).Select(r => r.Nombre).ToList() }
                }
            });
        }

        /// <summary>
        /// Verifica si un propietario puede ser eliminado
        /// </summary>
        [HttpGet("api/v1/Integridad/propietario/{id}/dependencias")]
        public async Task<IActionResult> DependenciasPropietario(string id)
        {
            var prop = await propietarioRepo.FindOneAsync(id);
            if (prop == null) return NotFound();

            var pacientes = (await pacienteRepo.FindAllAsync())
                .Where(p => p.PropietarioId == id).ToList();
            var ventas = (await ventaRepo.FindAllAsync())
                .Where(v => v.PropietarioId == id).ToList();
            var turnos = (await turnoRepo.FindAllAsync())
                .Where(t => pacientes.Any(p => p.Id == t.PacienteId)).ToList();

            return Ok(new DependencyCheckResult
            {
                EntidadId = id,
                EntidadTipo = "Propietario",
                EntidadNombre = $"{prop.Apellido}, {prop.Nombre}",
                PuedeEliminar = !pacientes.Any() && !ventas.Any(),
                Dependencias = new List<DependencyDetail>
                {
                    new() { Tipo = "Pacientes", Cantidad = pacientes.Count,
                        Items = pacientes.Take(5).Select(p => p.Nombre).ToList() },
                    new() { Tipo = "Ventas", Cantidad = ventas.Count,
                        Items = ventas.Take(5).Select(v => $"${v.Total} ({v.Fecha:dd/MM/yyyy})").ToList() },
                    new() { Tipo = "Turnos", Cantidad = turnos.Count }
                }
            });
        }

        /// <summary>
        /// Verifica si un paciente puede ser eliminado
        /// </summary>
        [HttpGet("api/v1/Integridad/paciente/{id}/dependencias")]
        public async Task<IActionResult> DependenciasPaciente(string id)
        {
            var pac = await pacienteRepo.FindOneAsync(id);
            if (pac == null) return NotFound();

            var turnos = (await turnoRepo.FindAllAsync())
                .Where(t => t.PacienteId == id).ToList();

            return Ok(new DependencyCheckResult
            {
                EntidadId = id,
                EntidadTipo = "Paciente",
                EntidadNombre = pac.Nombre,
                PuedeEliminar = !turnos.Any(t => t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado),
                Dependencias = new List<DependencyDetail>
                {
                    new() { Tipo = "Turnos activos", Cantidad = turnos.Count(t =>
                        t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado) },
                    new() { Tipo = "Total turnos", Cantidad = turnos.Count }
                }
            });
        }

        /// <summary>
        /// Verifica si un producto puede ser eliminado
        /// </summary>
        [HttpGet("api/v1/Integridad/producto/{id}/dependencias")]
        public async Task<IActionResult> DependenciasProducto(string id)
        {
            var prod = await productoRepo.FindOneAsync(id);
            if (prod == null) return NotFound();

            var ventas = (await ventaRepo.FindAllAsync())
                .Where(v => v.Detalles != null && v.Detalles.Any(d => d.ProductoId == id)).ToList();

            return Ok(new DependencyCheckResult
            {
                EntidadId = id,
                EntidadTipo = "Producto",
                EntidadNombre = prod.Nombre,
                PuedeEliminar = !ventas.Any(),
                Dependencias = new List<DependencyDetail>
                {
                    new() { Tipo = "Ventas con este producto", Cantidad = ventas.Count },
                    new() { Tipo = "Stock actual", Cantidad = prod.StockActual }
                }
            });
        }

        /// <summary>
        /// Verifica si un veterinario puede ser eliminado
        /// </summary>
        [HttpGet("api/v1/Integridad/veterinario/{id}/dependencias")]
        public async Task<IActionResult> DependenciasVeterinario(string id)
        {
            var vet = await veterinarioRepo.FindAllAsync();
            var veterinario = vet.FirstOrDefault(v => v.Id.ToString() == id);
            if (veterinario == null) return NotFound();

            var turnos = (await turnoRepo.FindAllAsync())
                .Where(t => t.VeterinarioId == id).ToList();
            var turnosFuturos = turnos.Where(t => t.FechaHora > DateTime.Now &&
                (t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado)).ToList();

            return Ok(new DependencyCheckResult
            {
                EntidadId = id,
                EntidadTipo = "Veterinario",
                EntidadNombre = $"Dr. {veterinario.Apellido}, {veterinario.Nombre}",
                PuedeEliminar = !turnosFuturos.Any(),
                Dependencias = new List<DependencyDetail>
                {
                    new() { Tipo = "Turnos futuros", Cantidad = turnosFuturos.Count,
                        Items = turnosFuturos.Take(5).Select(t => $"{t.FechaHora:dd/MM HH:mm}").ToList() },
                    new() { Tipo = "Total turnos históricos", Cantidad = turnos.Count }
                }
            });
        }

        /// <summary>
        /// Verifica si un servicio puede ser eliminado
        /// </summary>
        [HttpGet("api/v1/Integridad/servicio/{id}/dependencias")]
        public async Task<IActionResult> DependenciasServicio(int id)
        {
            var servicio = await servicioRepo.FindOneAsync(id);
            if (servicio == null) return NotFound();

            var turnos = (await turnoRepo.FindAllAsync())
                .Where(t => t.ServicioId == id).ToList();

            return Ok(new DependencyCheckResult
            {
                EntidadId = id.ToString(),
                EntidadTipo = "Servicio",
                EntidadNombre = servicio.Nombre,
                PuedeEliminar = !turnos.Any(t =>
                    t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado),
                Dependencias = new List<DependencyDetail>
                {
                    new() { Tipo = "Turnos activos", Cantidad = turnos.Count(t =>
                        t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado) },
                    new() { Tipo = "Total turnos históricos", Cantidad = turnos.Count }
                }
            });
        }

        // ═══════════════════════════════
        //  VALIDACIÓN GENERAL DEL SISTEMA
        // ═══════════════════════════════

        /// <summary>
        /// Ejecuta validaciones de integridad en toda la base de datos
        /// </summary>
        [HttpGet("api/v1/Integridad/validar")]
        public async Task<IActionResult> ValidarIntegridad()
        {
            var problemas = new List<string>();

            // Pacientes sin propietario válido
            var pacientes = await pacienteRepo.FindAllAsync();
            var propietarios = await propietarioRepo.FindAllAsync();
            var propIds = propietarios.Select(p => p.Id).ToHashSet();
            var pacSinProp = pacientes.Where(p => !propIds.Contains(p.PropietarioId)).ToList();
            if (pacSinProp.Any())
                problemas.Add($"⚠️ {pacSinProp.Count} pacientes con propietario inexistente");

            // Productos con stock negativo
            var productos = (await productoRepo.FindAllAsync())
                .Where(p => p.Activo && p.StockActual < 0).ToList();
            if (productos.Any())
                problemas.Add($"⚠️ {productos.Count} productos con stock negativo");

            // Turnos con fecha pasada en estado Programado
            var turnosPasados = (await turnoRepo.FindAllAsync())
                .Where(t => t.FechaHora < DateTime.Now.AddHours(-2) &&
                    (t.Estado == EstadoTurno.Programado || t.Estado == EstadoTurno.Confirmado))
                .ToList();
            if (turnosPasados.Any())
                problemas.Add($"⚠️ {turnosPasados.Count} turnos pasados aún en estado Programado/Confirmado");

            return Ok(new
            {
                FechaValidacion = DateTime.Now,
                Estado = problemas.Any() ? "Problemas detectados" : "Todo OK ✅",
                TotalProblemas = problemas.Count,
                Problemas = problemas
            });
        }
    }

    // ═══════════════════════════════
    //  DTOs
    // ═══════════════════════════════

    public class DependencyCheckResult
    {
        public string EntidadId { get; set; }
        public string EntidadTipo { get; set; }
        public string EntidadNombre { get; set; }
        public bool PuedeEliminar { get; set; }
        public List<DependencyDetail> Dependencias { get; set; } = new();
    }

    public class DependencyDetail
    {
        public string Tipo { get; set; }
        public int Cantidad { get; set; }
        public List<string> Items { get; set; } = new();
    }
}
