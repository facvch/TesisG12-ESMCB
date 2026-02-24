using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestionar las Razas de animales
    /// </summary>
    [ApiController]
    public class RazaController(IRazaRepository razaRepository, IEspecieRepository especieRepository) : BaseController
    {
        private readonly IRazaRepository _razaRepository = razaRepository 
            ?? throw new ArgumentNullException(nameof(razaRepository));
        private readonly IEspecieRepository _especieRepository = especieRepository 
            ?? throw new ArgumentNullException(nameof(especieRepository));

        /// <summary>
        /// Obtiene todas las razas
        /// </summary>
        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll(bool soloActivas = true)
        {
            var entities = soloActivas 
                ? await _razaRepository.GetActivasAsync() 
                : await _razaRepository.FindAllAsync();

            var dtos = entities.Select(r => new RazaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                EspecieId = r.EspecieId,
                EspecieNombre = r.Especie?.Nombre ?? "",
                Activo = r.Activo
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene razas por especie
        /// </summary>
        [HttpGet("api/v1/[Controller]/byEspecie/{especieId}")]
        public async Task<IActionResult> GetByEspecie(int especieId)
        {
            if (especieId <= 0) return BadRequest("El ID de especie debe ser mayor a 0");

            var entities = await _razaRepository.GetByEspecieIdAsync(especieId);

            var dtos = entities.Select(r => new RazaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                EspecieId = r.EspecieId,
                Activo = r.Activo
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene una raza por su ID
        /// </summary>
        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("El ID debe ser mayor a 0");

            var entity = await _razaRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró la raza con Id {id}");

            return Ok(new RazaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Descripcion = entity.Descripcion,
                EspecieId = entity.EspecieId,
                Activo = entity.Activo
            });
        }

        /// <summary>
        /// Crea una nueva raza
        /// </summary>
        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreateRazaRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var especie = await _especieRepository.FindOneAsync(request.EspecieId);
            if (especie == null) return BadRequest($"No existe la especie con Id {request.EspecieId}");

            var entity = new Domain.Entities.Raza(request.Nombre, request.EspecieId, request.Descripcion ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _razaRepository.AddAsync(entity);
            return Created($"api/v1/Raza/{createdId}", new { Id = createdId });
        }

        /// <summary>
        /// Actualiza una raza existente
        /// </summary>
        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdateRazaRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var entity = await _razaRepository.FindOneAsync(request.Id);
            if (entity == null) return NotFound($"No se encontró la raza con Id {request.Id}");

            entity.Actualizar(request.Nombre, request.Descripcion ?? "");
            if (request.EspecieId > 0) entity.CambiarEspecie(request.EspecieId);

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            _razaRepository.Update(request.Id, entity);
            return NoContent();
        }

        /// <summary>
        /// Elimina (desactiva) una raza
        /// </summary>
        [HttpDelete("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("El ID debe ser mayor a 0");

            var entity = await _razaRepository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró la raza con Id {id}");

            entity.Desactivar();
            _razaRepository.Update(id, entity);
            return NoContent();
        }
    }

    public class CreateRazaRequest
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int EspecieId { get; set; }
    }

    public class UpdateRazaRequest
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int EspecieId { get; set; }
    }
}
