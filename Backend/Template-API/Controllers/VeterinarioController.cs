using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    public class VeterinarioController(IVeterinarioRepository veterinarioRepository) : BaseController
    {
        private readonly IVeterinarioRepository _repository = veterinarioRepository
            ?? throw new ArgumentNullException(nameof(veterinarioRepository));

        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll(bool soloActivos = true)
        {
            var entities = soloActivos
                ? await _repository.GetActivosAsync()
                : await _repository.FindAllAsync();

            var dtos = entities.Select(MapToDto).ToList();
            return Ok(new QueryResult<VeterinarioDto>(dtos, dtos.Count, 1, 10));
        }

        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("El ID es requerido");
            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el veterinario con Id {id}");
            return Ok(MapToDto(entity));
        }

        [HttpGet("api/v1/[Controller]/search")]
        public async Task<IActionResult> Search([FromQuery] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return BadRequest("Debe proporcionar un término de búsqueda");
            var entities = await _repository.SearchByNombreAsync(nombre);
            return Ok(entities.Select(MapToDto).ToList());
        }

        [HttpGet("api/v1/[Controller]/byMatricula/{matricula}")]
        public async Task<IActionResult> GetByMatricula(string matricula)
        {
            var entity = await _repository.GetByMatriculaAsync(matricula);
            if (entity == null) return NotFound($"No se encontró veterinario con matrícula {matricula}");
            return Ok(MapToDto(entity));
        }

        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateVeterinarioRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var existing = await _repository.GetByMatriculaAsync(request.Matricula);
            if (existing != null) return BadRequest($"Ya existe un veterinario con la matrícula '{request.Matricula}'");

            var entity = new Domain.Entities.Veterinario(
                request.Nombre, request.Apellido, request.Matricula,
                request.Telefono, request.Email ?? "", request.Especialidad ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _repository.AddAsync(entity);
            return Created($"api/v1/Veterinario/{createdId}", new { Id = createdId });
        }

        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateVeterinarioRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");
            var entity = await _repository.FindOneAsync(request.Id);
            if (entity == null) return NotFound($"No se encontró el veterinario con Id {request.Id}");

            entity.Actualizar(request.Nombre, request.Apellido, request.Telefono,
                request.Email ?? "", request.Especialidad ?? "");

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
            if (entity == null) return NotFound($"No se encontró el veterinario con Id {id}");

            entity.Desactivar();
            _repository.Update(id, entity);
            return NoContent();
        }

        private static VeterinarioDto MapToDto(Domain.Entities.Veterinario v) => new()
        {
            Id = v.Id, Nombre = v.Nombre, Apellido = v.Apellido,
            NombreCompleto = v.NombreCompleto, Matricula = v.Matricula,
            Telefono = v.Telefono, Email = v.Email,
            Especialidad = v.Especialidad, Activo = v.Activo
        };
    }

    public class CreateVeterinarioRequest
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Matricula { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Especialidad { get; set; }
    }

    public class UpdateVeterinarioRequest
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Especialidad { get; set; }
    }
}
