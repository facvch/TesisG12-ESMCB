using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    public class AuditController(
        IAuditLogRepository auditRepo,
        INotificacionRepository notificacionRepo,
        IRegistroVacunacionRepository vacRepo,
        IProductoRepository productoRepo,
        ITurnoRepository turnoRepo) : BaseController
    {
        private readonly IAuditLogRepository _auditRepo = auditRepo;
        private readonly INotificacionRepository _notifRepo = notificacionRepo;
        private readonly IRegistroVacunacionRepository _vacRepo = vacRepo;
        private readonly IProductoRepository _productoRepo = productoRepo;
        private readonly ITurnoRepository _turnoRepo = turnoRepo;

        // ═══════════════════════════════════════════
        // AUDITORÍA
        // ═══════════════════════════════════════════

        /// <summary>
        /// Últimas acciones del sistema
        /// </summary>
        [HttpGet("api/v1/Audit/logs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLogs([FromQuery] int cantidad = 50)
        {
            var logs = await _auditRepo.GetUltimosAsync(Math.Min(cantidad, 200));
            return Ok(logs.Select(MapAuditDto).ToList());
        }

        /// <summary>
        /// Logs por rango de fechas
        /// </summary>
        [HttpGet("api/v1/Audit/logs/fecha")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLogsByFecha(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var d = desde ?? DateTime.Today.AddDays(-7);
            var h = hasta ?? DateTime.Now;
            var logs = await _auditRepo.GetByFechaRangoAsync(d, h);
            return Ok(logs.Select(MapAuditDto).ToList());
        }

        /// <summary>
        /// Logs de un usuario específico
        /// </summary>
        [HttpGet("api/v1/Audit/logs/usuario/{usuarioId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLogsByUsuario(string usuarioId)
        {
            var logs = await _auditRepo.GetByUsuarioIdAsync(usuarioId);
            return Ok(logs.Select(MapAuditDto).ToList());
        }

        /// <summary>
        /// Logs de una entidad específica
        /// </summary>
        [HttpGet("api/v1/Audit/logs/entidad/{entidad}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLogsByEntidad(string entidad)
        {
            var logs = await _auditRepo.GetByEntidadAsync(entidad);
            return Ok(logs.Select(MapAuditDto).ToList());
        }

        // ═══════════════════════════════════════════
        // NOTIFICACIONES
        // ═══════════════════════════════════════════

        /// <summary>
        /// Obtener notificaciones no leídas
        /// </summary>
        [HttpGet("api/v1/Notificacion/noLeidas")]
        public async Task<IActionResult> GetNoLeidas()
        {
            var notifs = await _notifRepo.GetNoLeidasAsync();
            return Ok(notifs.Select(MapNotifDto).ToList());
        }

        /// <summary>
        /// Obtener últimas notificaciones
        /// </summary>
        [HttpGet("api/v1/Notificacion")]
        public async Task<IActionResult> GetUltimas([FromQuery] int cantidad = 20)
        {
            var notifs = await _notifRepo.GetUltimasAsync(Math.Min(cantidad, 100));
            return Ok(notifs.Select(MapNotifDto).ToList());
        }

        /// <summary>
        /// Marcar notificación como leída
        /// </summary>
        [HttpPut("api/v1/Notificacion/{id}/leer")]
        public async Task<IActionResult> MarcarLeida(string id)
        {
            var notif = await _notifRepo.FindOneAsync(id);
            if (notif == null) return NotFound();
            notif.MarcarLeida();
            _notifRepo.Update(id, notif);
            return NoContent();
        }

        /// <summary>
        /// Generar notificaciones automáticas (vacunas vencidas, stock bajo, turnos próximos)
        /// </summary>
        [HttpPost("api/v1/Notificacion/generar")]
        public async Task<IActionResult> GenerarNotificaciones()
        {
            int count = 0;

            // 1. Vacunas próximas a vencer (próximos 30 días)
            var vacunas = await _vacRepo.FindAllAsync();
            foreach (var vac in vacunas)
            {
                if (vac.FechaProximaDosis != null && vac.FechaProximaDosis <= DateTime.Now.AddDays(30)
                    && vac.FechaProximaDosis >= DateTime.Now.AddDays(-7))
                {
                    var notif = new Notificacion(
                        "Vacuna próxima a vencer",
                        $"La vacuna del paciente (registro {vac.Id}) vence el {vac.FechaProximaDosis:dd/MM/yyyy}",
                        TipoNotificacion.VacunaPendiente,
                        "RegistroVacunacion", vac.Id.ToString());
                    await _notifRepo.AddAsync(notif);
                    count++;
                }
            }

            // 2. Stock bajo
            var productos = await _productoRepo.FindAllAsync();
            foreach (var prod in productos)
            {
                if (prod.Activo && prod.StockActual <= prod.StockMinimo)
                {
                    var notif = new Notificacion(
                        "Stock bajo",
                        $"El producto '{prod.Nombre}' tiene stock bajo ({prod.StockActual}/{prod.StockMinimo})",
                        TipoNotificacion.StockBajo,
                        "Producto", prod.Id);
                    await _notifRepo.AddAsync(notif);
                    count++;
                }
            }

            // 3. Turnos de mañana
            var manana = DateTime.Today.AddDays(1);
            var turnos = await _turnoRepo.FindAllAsync();
            foreach (var turno in turnos.Where(t => t.FechaHora.Date == manana && t.Estado == 0))
            {
                var notif = new Notificacion(
                    "Turno mañana",
                    $"Turno programado para mañana a las {turno.FechaHora:HH:mm}",
                    TipoNotificacion.TurnoProximo,
                    "Turno", turno.Id.ToString());
                await _notifRepo.AddAsync(notif);
                count++;
            }

            return Ok(new { NotificacionesGeneradas = count });
        }

        // ═══════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════

        private static AuditLogDto MapAuditDto(AuditLog a) => new()
        {
            Id = a.Id, UsuarioId = a.UsuarioId, NombreUsuario = a.NombreUsuario,
            Accion = a.Accion, Entidad = a.Entidad, EntidadId = a.EntidadId,
            Descripcion = a.Descripcion, IpOrigen = a.IpOrigen,
            Fecha = a.Fecha, StatusCode = a.StatusCode
        };

        private static NotificacionDto MapNotifDto(Notificacion n) => new()
        {
            Id = n.Id, Titulo = n.Titulo, Mensaje = n.Mensaje,
            Tipo = n.Tipo.ToString(), EntidadRelacionada = n.EntidadRelacionada,
            EntidadId = n.EntidadId, FechaCreacion = n.FechaCreacion,
            Leida = n.Leida, FechaLectura = n.FechaLectura
        };
    }
}
