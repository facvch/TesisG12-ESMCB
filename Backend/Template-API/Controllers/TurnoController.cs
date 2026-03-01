using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestionar Turnos con validación de disponibilidad
    /// </summary>
    [ApiController]
    public class TurnoController(
        ITurnoRepository turnoRepository,
        IPacienteRepository pacienteRepository,
        IVeterinarioRepository veterinarioRepository,
        IServicioRepository servicioRepository) : BaseController
    {
        private readonly ITurnoRepository _turnoRepository = turnoRepository
            ?? throw new ArgumentNullException(nameof(turnoRepository));
        private readonly IPacienteRepository _pacienteRepository = pacienteRepository
            ?? throw new ArgumentNullException(nameof(pacienteRepository));
        private readonly IVeterinarioRepository _veterinarioRepository = veterinarioRepository
            ?? throw new ArgumentNullException(nameof(veterinarioRepository));
        private readonly IServicioRepository _servicioRepository = servicioRepository
            ?? throw new ArgumentNullException(nameof(servicioRepository));

        /// <summary>
        /// Obtiene la agenda de un día (todos los turnos)
        /// </summary>
        [HttpGet("api/v1/[Controller]/agenda")]
        public async Task<IActionResult> GetAgenda([FromQuery] DateTime? fecha)
        {
            var dia = fecha ?? DateTime.Today;
            var entities = await _turnoRepository.GetByFechaAsync(dia);
            var dtos = entities.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene turnos programados en un rango de fechas
        /// </summary>
        [HttpGet("api/v1/[Controller]/programados")]
        public async Task<IActionResult> GetProgramados([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            if (hasta <= desde) return BadRequest("La fecha 'hasta' debe ser posterior a 'desde'");

            var entities = await _turnoRepository.GetProgramadosAsync(desde, hasta);
            var dtos = entities.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene turnos de un veterinario
        /// </summary>
        [HttpGet("api/v1/[Controller]/byVeterinario/{veterinarioId}")]
        public async Task<IActionResult> GetByVeterinario(string veterinarioId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            if (string.IsNullOrWhiteSpace(veterinarioId)) return BadRequest("El ID del veterinario es requerido");

            var entities = await _turnoRepository.GetByVeterinarioIdAsync(veterinarioId, desde, hasta);
            var dtos = entities.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene turnos de un paciente
        /// </summary>
        [HttpGet("api/v1/[Controller]/byPaciente/{pacienteId}")]
        public async Task<IActionResult> GetByPaciente(string pacienteId)
        {
            if (string.IsNullOrWhiteSpace(pacienteId)) return BadRequest("El ID del paciente es requerido");

            var entities = await _turnoRepository.GetByPacienteIdAsync(pacienteId);
            var dtos = entities.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un turno por su ID
        /// </summary>
        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("El ID es requerido");
            var entity = await _turnoRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el turno con Id {id}");
            return Ok(MapToDto(entity));
        }

        /// <summary>
        /// Crea un nuevo turno con validación de superposición
        /// </summary>
        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateTurnoRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            // Validar paciente
            var paciente = await _pacienteRepository.FindOneAsync(request.PacienteId);
            if (paciente == null) return BadRequest($"No existe el paciente con Id {request.PacienteId}");

            // Validar veterinario
            var veterinario = await _veterinarioRepository.FindOneAsync(request.VeterinarioId);
            if (veterinario == null) return BadRequest($"No existe el veterinario con Id {request.VeterinarioId}");

            // Validar servicio
            var servicio = await _servicioRepository.FindOneAsync(request.ServicioId);
            if (servicio == null) return BadRequest($"No existe el servicio con Id {request.ServicioId}");

            // Duración: usar la del servicio si no se especifica
            var duracion = request.DuracionMinutos > 0 ? request.DuracionMinutos : servicio.DuracionMinutos;

            // Validar superposición con turnos del veterinario
            var turnosVet = await _turnoRepository.GetByVeterinarioIdAsync(
                request.VeterinarioId, request.FechaHora.Date, request.FechaHora.Date.AddDays(1));

            var turnosActivos = turnosVet.Where(t =>
                t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Ausente);

            foreach (var turnoExistente in turnosActivos)
            {
                if (turnoExistente.SeSuperponeCon(request.FechaHora, duracion))
                {
                    return BadRequest($"El veterinario ya tiene un turno entre " +
                        $"{turnoExistente.FechaHora:HH:mm} y {turnoExistente.FechaHoraFin:HH:mm}");
                }
            }

            var entity = new Domain.Entities.Turno(
                request.PacienteId, request.VeterinarioId, request.ServicioId,
                request.FechaHora, duracion, request.Motivo ?? "", request.Observaciones ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _turnoRepository.AddAsync(entity);
            return Created($"api/v1/Turno/{createdId}", new { Id = createdId });
        }

        /// <summary>
        /// Reprogramar un turno
        /// </summary>
        [HttpPut("api/v1/[Controller]/{id}/reprogramar")]
        public async Task<IActionResult> Reprogramar(string id, [FromBody] ReprogramarTurnoRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var entity = await _turnoRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el turno con Id {id}");

            if (entity.Estado == EstadoTurno.Completado || entity.Estado == EstadoTurno.Cancelado)
                return BadRequest("No se puede reprogramar un turno completado o cancelado");

            // Validar superposición
            var duracion = request.DuracionMinutos > 0 ? request.DuracionMinutos : entity.DuracionMinutos;
            var turnosVet = await _turnoRepository.GetByVeterinarioIdAsync(
                entity.VeterinarioId, request.NuevaFechaHora.Date, request.NuevaFechaHora.Date.AddDays(1));

            var turnosActivos = turnosVet.Where(t =>
                t.Id != id && t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Ausente);

            foreach (var turnoExistente in turnosActivos)
            {
                if (turnoExistente.SeSuperponeCon(request.NuevaFechaHora, duracion))
                {
                    return BadRequest($"El veterinario ya tiene un turno entre " +
                        $"{turnoExistente.FechaHora:HH:mm} y {turnoExistente.FechaHoraFin:HH:mm}");
                }
            }

            entity.Reprogramar(request.NuevaFechaHora, duracion);
            _turnoRepository.Update(id, entity);
            return NoContent();
        }

        /// <summary>
        /// Confirmar turno
        /// </summary>
        [HttpPut("api/v1/[Controller]/{id}/confirmar")]
        public async Task<IActionResult> Confirmar(string id)
        {
            var entity = await _turnoRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el turno con Id {id}");
            entity.Confirmar();
            _turnoRepository.Update(id, entity);
            return NoContent();
        }

        /// <summary>
        /// Marcar turno en curso
        /// </summary>
        [HttpPut("api/v1/[Controller]/{id}/encurso")]
        public async Task<IActionResult> EnCurso(string id)
        {
            var entity = await _turnoRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el turno con Id {id}");
            entity.EnCurso();
            _turnoRepository.Update(id, entity);
            return NoContent();
        }

        /// <summary>
        /// Completar turno
        /// </summary>
        [HttpPut("api/v1/[Controller]/{id}/completar")]
        public async Task<IActionResult> Completar(string id, [FromBody] string observaciones = "")
        {
            var entity = await _turnoRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el turno con Id {id}");
            entity.Completar(observaciones);
            _turnoRepository.Update(id, entity);
            return NoContent();
        }

        /// <summary>
        /// Cancelar turno
        /// </summary>
        [HttpPut("api/v1/[Controller]/{id}/cancelar")]
        public async Task<IActionResult> Cancelar(string id, [FromBody] string motivo = "")
        {
            var entity = await _turnoRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el turno con Id {id}");
            entity.Cancelar(motivo);
            _turnoRepository.Update(id, entity);
            return NoContent();
        }

        /// <summary>
        /// Marcar como ausente
        /// </summary>
        [HttpPut("api/v1/[Controller]/{id}/ausente")]
        public async Task<IActionResult> Ausente(string id)
        {
            var entity = await _turnoRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el turno con Id {id}");
            entity.Ausente();
            _turnoRepository.Update(id, entity);
            return NoContent();
        }

        private static TurnoDto MapToDto(Domain.Entities.Turno t) => new()
        {
            Id = t.Id,
            PacienteId = t.PacienteId,
            PacienteNombre = t.Paciente?.Nombre ?? "",
            VeterinarioId = t.VeterinarioId,
            VeterinarioNombre = t.Veterinario?.NombreCompleto ?? "",
            ServicioId = t.ServicioId,
            ServicioNombre = t.Servicio?.Nombre ?? "",
            FechaHora = t.FechaHora,
            FechaHoraFin = t.FechaHoraFin,
            DuracionMinutos = t.DuracionMinutos,
            Estado = t.Estado.ToString(),
            Motivo = t.Motivo,
            Observaciones = t.Observaciones,
            FechaCreacion = t.FechaCreacion
        };
    }

    public class CreateTurnoRequest
    {
        public string PacienteId { get; set; }
        public string VeterinarioId { get; set; }
        public int ServicioId { get; set; }
        public DateTime FechaHora { get; set; }
        public int DuracionMinutos { get; set; }
        public string Motivo { get; set; }
        public string Observaciones { get; set; }
    }

    public class ReprogramarTurnoRequest
    {
        public DateTime NuevaFechaHora { get; set; }
        public int DuracionMinutos { get; set; }
    }
}
