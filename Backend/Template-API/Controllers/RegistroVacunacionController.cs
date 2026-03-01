using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestionar los registros de vacunación
    /// </summary>
    [ApiController]
    public class RegistroVacunacionController(
        IRegistroVacunacionRepository registroRepository,
        IPacienteRepository pacienteRepository,
        IVacunaRepository vacunaRepository) : BaseController
    {
        private readonly IRegistroVacunacionRepository _registroRepository = registroRepository
            ?? throw new ArgumentNullException(nameof(registroRepository));
        private readonly IPacienteRepository _pacienteRepository = pacienteRepository
            ?? throw new ArgumentNullException(nameof(pacienteRepository));
        private readonly IVacunaRepository _vacunaRepository = vacunaRepository
            ?? throw new ArgumentNullException(nameof(vacunaRepository));

        /// <summary>
        /// Obtiene los registros de vacunación de un paciente
        /// </summary>
        [HttpGet("api/v1/[Controller]/byPaciente/{pacienteId}")]
        public async Task<IActionResult> GetByPaciente(string pacienteId)
        {
            if (string.IsNullOrWhiteSpace(pacienteId)) return BadRequest("El ID del paciente es requerido");

            var paciente = await _pacienteRepository.FindOneAsync(pacienteId);
            if (paciente == null) return NotFound($"No se encontró el paciente con Id {pacienteId}");

            var registros = await _registroRepository.GetByPacienteIdAsync(pacienteId);
            var dtos = registros.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene vacunas con próxima dosis vencida (alertas)
        /// </summary>
        [HttpGet("api/v1/[Controller]/pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            var registros = await _registroRepository.GetVacunasPendientesAsync();
            var dtos = registros.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un registro de vacunación por ID
        /// </summary>
        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("El ID es requerido");

            var entity = await _registroRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el registro con Id {id}");

            return Ok(MapToDto(entity));
        }

        /// <summary>
        /// Registra una vacunación aplicada
        /// </summary>
        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateRegistroVacunacionRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var paciente = await _pacienteRepository.FindOneAsync(request.PacienteId);
            if (paciente == null) return BadRequest($"No existe el paciente con Id {request.PacienteId}");

            var vacuna = await _vacunaRepository.FindOneAsync(request.VacunaId);
            if (vacuna == null) return BadRequest($"No existe la vacuna con Id {request.VacunaId}");

            // Calcular próxima dosis automáticamente si la vacuna tiene intervalo definido
            DateTime? fechaProximaDosis = request.FechaProximaDosis;
            if (!fechaProximaDosis.HasValue && vacuna.IntervaloDosisDias.HasValue)
            {
                fechaProximaDosis = request.FechaAplicacion.AddDays(vacuna.IntervaloDosisDias.Value);
            }

            var entity = new Domain.Entities.RegistroVacunacion(
                request.PacienteId,
                request.VacunaId,
                request.FechaAplicacion,
                request.Veterinario,
                request.NroLote ?? "",
                fechaProximaDosis,
                request.Observaciones ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _registroRepository.AddAsync(entity);
            return Created($"api/v1/RegistroVacunacion/{createdId}", new { Id = createdId });
        }

        /// <summary>
        /// Actualiza un registro de vacunación
        /// </summary>
        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateRegistroVacunacionRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var entity = await _registroRepository.FindOneAsync(request.Id);
            if (entity == null) return NotFound($"No se encontró el registro con Id {request.Id}");

            entity.Actualizar(request.FechaAplicacion, request.Veterinario, request.NroLote ?? "",
                request.FechaProximaDosis, request.Observaciones ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            _registroRepository.Update(request.Id, entity);
            return NoContent();
        }

        private static RegistroVacunacionDto MapToDto(Domain.Entities.RegistroVacunacion r) => new()
        {
            Id = r.Id,
            PacienteId = r.PacienteId,
            PacienteNombre = r.Paciente?.Nombre ?? "",
            VacunaId = r.VacunaId,
            VacunaNombre = r.Vacuna?.Nombre ?? "",
            FechaAplicacion = r.FechaAplicacion,
            FechaProximaDosis = r.FechaProximaDosis,
            ProximaDosisVencida = r.FechaProximaDosis.HasValue && r.FechaProximaDosis.Value <= DateTime.Now,
            Veterinario = r.Veterinario,
            NroLote = r.NroLote,
            Observaciones = r.Observaciones
        };
    }

    public class CreateRegistroVacunacionRequest
    {
        public string PacienteId { get; set; }
        public int VacunaId { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public DateTime? FechaProximaDosis { get; set; }
        public string Veterinario { get; set; }
        public string NroLote { get; set; }
        public string Observaciones { get; set; }
    }

    public class UpdateRegistroVacunacionRequest
    {
        public string Id { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public DateTime? FechaProximaDosis { get; set; }
        public string Veterinario { get; set; }
        public string NroLote { get; set; }
        public string Observaciones { get; set; }
    }
}
