using Application.DataTransferObjects;
using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    public class HorarioController(
        IHorarioRepository horarioRepository,
        ITipoHorarioRepository tipoHorarioRepository) : BaseController
    {
        private readonly IHorarioRepository _repository = horarioRepository
            ?? throw new ArgumentNullException(nameof(horarioRepository));
        private readonly ITipoHorarioRepository _tipoRepository = tipoHorarioRepository
            ?? throw new ArgumentNullException(nameof(tipoHorarioRepository));

        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _repository.GetActivosAsync();
            var dtos = entities.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("api/v1/[Controller]/veterinario/{veterinarioId}")]
        public async Task<IActionResult> GetByVeterinario(string veterinarioId)
        {
            if (string.IsNullOrWhiteSpace(veterinarioId)) return BadRequest("El id del veterinario es requerido");
            var entities = await _repository.GetByVeterinarioIdAsync(veterinarioId);
            var dtos = entities.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateHorarioRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            if (!TimeSpan.TryParse(request.HoraInicio, out var horaInicio) ||
                !TimeSpan.TryParse(request.HoraFin, out var horaFin))
            {
                return BadRequest("Formato de hora inválido. Debe ser HH:mm");
            }

            var entity = new Horario(
                request.VeterinarioId,
                request.DiaSemana,
                horaInicio,
                horaFin,
                request.TipoHorarioId);

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _repository.AddAsync(entity);
            return Created($"api/v1/Horario/{createdId}", new { Id = createdId });
        }

        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateHorarioRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");
            var entity = await _repository.FindOneAsync(request.Id);
            if (entity == null) return NotFound($"No se encontró el horario con Id {request.Id}");

            if (!TimeSpan.TryParse(request.HoraInicio, out var horaInicio) ||
                !TimeSpan.TryParse(request.HoraFin, out var horaFin))
            {
                return BadRequest("Formato de hora inválido. Debe ser HH:mm");
            }

            entity.Actualizar(request.DiaSemana, horaInicio, horaFin, request.TipoHorarioId);

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            _repository.Update(request.Id, entity);
            return NoContent();
        }

        [HttpDelete("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("El ID es requerido");
            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el horario con Id {id}");

            _repository.Remove(id);
            return NoContent();
        }

        private static HorarioDto MapToDto(Horario h) => new()
        {
            Id = h.Id,
            VeterinarioId = h.VeterinarioId,
            DiaSemana = h.DiaSemana,
            HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
            HoraFin = h.HoraFin.ToString(@"hh\:mm"),
            TipoHorarioId = h.TipoHorarioId,
            TipoHorarioNombre = h.TipoHorario?.Nombre ?? (h.TipoHorarioId == 2 ? "Guardia" : "Normal"),
            Activo = h.Activo
        };
    }
}
