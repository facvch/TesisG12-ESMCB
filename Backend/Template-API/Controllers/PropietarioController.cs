using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestionar los Propietarios de mascotas
    /// </summary>
    [ApiController]
    public class PropietarioController(IPropietarioRepository propietarioRepository) : BaseController
    {
        private readonly IPropietarioRepository _repository = propietarioRepository 
            ?? throw new ArgumentNullException(nameof(propietarioRepository));

        /// <summary>
        /// Obtiene todos los propietarios
        /// </summary>
        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll(bool soloActivos = true)
        {
            var entities = soloActivos 
                ? await _repository.GetActivosAsync() 
                : await _repository.FindAllAsync();

            var dtos = entities.Select(p => new PropietarioDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Apellido = p.Apellido,
                NombreCompleto = p.NombreCompleto,
                DNI = p.DNI,
                Telefono = p.Telefono,
                Email = p.Email,
                Direccion = p.Direccion,
                FechaRegistro = p.FechaRegistro,
                Activo = p.Activo,
                CantidadMascotas = p.Mascotas?.Count ?? 0
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Busca propietarios por nombre
        /// </summary>
        [HttpGet("api/v1/[Controller]/search")]
        public async Task<IActionResult> Search([FromQuery] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) 
                return BadRequest("Debe proporcionar un término de búsqueda");

            var entities = await _repository.SearchByNombreAsync(nombre);

            var dtos = entities.Select(p => new PropietarioDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Apellido = p.Apellido,
                NombreCompleto = p.NombreCompleto,
                DNI = p.DNI,
                Telefono = p.Telefono,
                Activo = p.Activo
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un propietario por su ID
        /// </summary>
        [HttpGet("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("El ID es requerido");

            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el propietario con Id {id}");

            return Ok(new PropietarioDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Apellido = entity.Apellido,
                NombreCompleto = entity.NombreCompleto,
                DNI = entity.DNI,
                Telefono = entity.Telefono,
                Email = entity.Email,
                Direccion = entity.Direccion,
                FechaRegistro = entity.FechaRegistro,
                Activo = entity.Activo,
                CantidadMascotas = entity.Mascotas?.Count ?? 0
            });
        }

        /// <summary>
        /// Busca un propietario por DNI
        /// </summary>
        [HttpGet("api/v1/[Controller]/byDni/{dni}")]
        public async Task<IActionResult> GetByDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni)) return BadRequest("El DNI es requerido");

            var entity = await _repository.GetByDNIAsync(dni);
            if (entity == null) return NotFound($"No se encontró el propietario con DNI {dni}");

            return Ok(new PropietarioDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Apellido = entity.Apellido,
                NombreCompleto = entity.NombreCompleto,
                DNI = entity.DNI,
                Telefono = entity.Telefono,
                Email = entity.Email,
                Activo = entity.Activo
            });
        }

        /// <summary>
        /// Crea un nuevo propietario
        /// </summary>
        [HttpPost("api/v1/[Controller]")]
        public async Task<IActionResult> Create([FromBody] CreatePropietarioRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            // Verificar DNI único
            var existing = await _repository.GetByDNIAsync(request.DNI);
            if (existing != null) return BadRequest($"Ya existe un propietario con DNI {request.DNI}");

            var entity = new Domain.Entities.Propietario(
                request.Nombre, 
                request.Apellido, 
                request.DNI, 
                request.Telefono, 
                request.Email ?? "", 
                request.Direccion ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            var createdId = await _repository.AddAsync(entity);
            return Created($"api/v1/Propietario/{createdId}", new { Id = createdId });
        }

        /// <summary>
        /// Actualiza un propietario existente
        /// </summary>
        [HttpPut("api/v1/[Controller]")]
        public async Task<IActionResult> Update([FromBody] UpdatePropietarioRequest request)
        {
            if (request is null) return BadRequest("El request no puede ser nulo");

            var entity = await _repository.FindOneAsync(request.Id);
            if (entity == null) return NotFound($"No se encontró el propietario con Id {request.Id}");

            entity.Actualizar(request.Nombre, request.Apellido, request.Telefono, request.Email ?? "", request.Direccion ?? "");

            if (!entity.IsValid)
                return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));

            _repository.Update(request.Id, entity);
            return NoContent();
        }

        /// <summary>
        /// Elimina (desactiva) un propietario
        /// </summary>
        [HttpDelete("api/v1/[Controller]/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("El ID es requerido");

            var entity = await _repository.FindOneAsync(id);
            if (entity == null) return NotFound($"No se encontró el propietario con Id {id}");

            entity.Desactivar();
            _repository.Update(id, entity);
            return NoContent();
        }
    }

    public class CreatePropietarioRequest
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
    }

    public class UpdatePropietarioRequest
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
    }
}
