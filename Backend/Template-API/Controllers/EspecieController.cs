using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestionar las Especies de animales
    /// </summary>
    [ApiController]
    public class EspecieController(IEspecieRepository especieRepository) : BaseController
    {
        private readonly IEspecieRepository _repository = especieRepository 
            ?? throw new ArgumentNullException(nameof(especieRepository));

        /// <summary>
        /// Obtiene todas las especies
        /// </summary>
        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll(uint pageIndex = 1, uint pageSize = 10, bool soloActivas = true)
        {
            var entities = soloActivas 
                ? await _repository.GetActivasAsync() 
                : await _repository.FindAllAsync();

            var dtos = entities.Select(e => new EspecieDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion,
                Activo = e.Activo
            }).ToList();

            return Ok(new QueryResult<EspecieDto>(dtos, dtos.Count, pageIndex, pageSize));
        }

        /// <summary>
        /// Obtiene una especie por su ID
        /// </summary>
        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("El ID debe ser mayor a 0");

            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró la especie con Id {id}");

            return Ok(new EspecieDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                Activo = entity.Activo
            });
        }

        /// <summary>
        /// Crea una nueva especie
        /// </summary>
        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateEspecieRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            // Verificar nombre único
            var existing = await _repository.GetByNombreAsync(request.Nombre);
            if (existing != null) return BadRequest($"Ya existe una especie con el nombre '{request.Nombre}'");

            var entity = new Domain.Entities.Especie(request.Nombre, request.Descripcion ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _repository.AddAsync(entity);
            return Created($"api/v1/Especie/{createdId}", new { Id = createdId });
        }

        /// <summary>
        /// Actualiza una especie existente
        /// </summary>
        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateEspecieRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var entity = await _repository.FindOneAsync(request.Id);
            if (entity == null) return NotFound($"No se encontró la especie con Id {request.Id}");

            entity.Actualizar(request.Nombre, request.Descripcion ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            _repository.Update(request.Id, entity);
            return NoContent();
        }

        /// <summary>
        /// Elimina (desactiva) una especie
        /// </summary>
        [HttpDelete("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("El ID debe ser mayor a 0");

            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró la especie con Id {id}");

            entity.Desactivar();
            _repository.Update(id, entity);
            return NoContent();
        }
    }

    public class CreateEspecieRequest
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class UpdateEspecieRequest
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
