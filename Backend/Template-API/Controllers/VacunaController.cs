using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestionar las Vacunas disponibles
    /// </summary>
    [ApiController]
    public class VacunaController(IVacunaRepository vacunaRepository) : BaseController
    {
        private readonly IVacunaRepository _repository = vacunaRepository
            ?? throw new ArgumentNullException(nameof(vacunaRepository));

        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll(bool soloActivas = true)
        {
            var entities = soloActivas
                ? await _repository.GetActivasAsync()
                : await _repository.FindAllAsync();

            var dtos = entities.Select(v => new VacunaDto
            {
                Id = v.Id,
                Nombre = v.Nombre,
                Descripcion = v.Descripcion,
                Laboratorio = v.Laboratorio,
                IntervaloDosisDias = v.IntervaloDosisDias,
                Activo = v.Activo
            }).ToList();

            return Ok(new QueryResult<VacunaDto>(dtos, dtos.Count, 1, 10));
        }

        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("El ID debe ser mayor a 0");

            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró la vacuna con Id {id}");

            return Ok(new VacunaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Laboratorio = entity.Laboratorio,
                IntervaloDosisDias = entity.IntervaloDosisDias,
                Activo = entity.Activo
            });
        }

        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateVacunaRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var existing = await _repository.GetByNombreAsync(request.Nombre);
            if (existing != null) return BadRequest($"Ya existe una vacuna con el nombre '{request.Nombre}'");

            var entity = new Domain.Entities.Vacuna(
                request.Nombre, request.Descripcion ?? "", request.Laboratorio ?? "", request.IntervaloDosisDias);

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _repository.AddAsync(entity);
            return Created($"api/v1/Vacuna/{createdId}", new { Id = createdId });
        }

        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateVacunaRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var entity = await _repository.FindOneAsync(request.Id);
            if (entity == null) return NotFound($"No se encontró la vacuna con Id {request.Id}");

            entity.Actualizar(request.Nombre, request.Descripcion ?? "", request.Laboratorio ?? "", request.IntervaloDosisDias);

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            _repository.Update(request.Id, entity);
            return NoContent();
        }

        [HttpDelete("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("El ID debe ser mayor a 0");

            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró la vacuna con Id {id}");

            entity.Desactivar();
            _repository.Update(id, entity);
            return NoContent();
        }
    }

    public class CreateVacunaRequest
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Laboratorio { get; set; }
        public int? IntervaloDosisDias { get; set; }
    }

    public class UpdateVacunaRequest
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Laboratorio { get; set; }
        public int? IntervaloDosisDias { get; set; }
    }
}
