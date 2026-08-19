using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    [Authorize]
    public class SucursalController(ISucursalRepository repo) : BaseController
    {
        private readonly ISucursalRepository _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        [HttpGet("api/v1/[Controller]")]
        [Authorize(Roles = "Admin,Gerente,Veterinario,Recepcionista")]
        public async Task<IActionResult> GetAll(bool soloActivos = true)
        {
            var entities = soloActivos ? await _repo.GetActivasAsync() : await _repo.FindAllAsync();
            var dtos = entities.Select(s => new SucursalDto
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Telefono = s.Telefono,
                Email = s.Email,
                Activa = s.Activa
            }).ToList();
            return Ok(new QueryResult<SucursalDto>(dtos, dtos.Count, 1, 10));
        }

        [HttpGet("api/v1/[Controller]/{id}")]
        [Authorize(Roles = "Admin,Gerente,Veterinario,Recepcionista")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _repo.FindOneAsync(id);
            if (s == null) return NotFound();
            return Ok(new SucursalDto
            {
                Id = s.Id,
                Nombre = s.Nombre,
                Direccion = s.Direccion,
                Telefono = s.Telefono,
                Email = s.Email,
                Activa = s.Activa
            });
        }

        [HttpPost("api/v1/[Controller]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSucursalRequest r)
        {
            var entity = new Sucursal(r.Nombre, r.Direccion, r.Telefono, r.Email ?? "");
            if (!entity.IsValid) return BadRequest(entity.GetErrors().Select(e => e.ErrorMessage));
            var id = await _repo.AddAsync(entity);
            return Created($"api/v1/Sucursal/{id}", new { Id = id });
        }

        [HttpPut("api/v1/[Controller]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] SucursalDto r)
        {
            var s = await _repo.FindOneAsync(r.Id);
            if (s == null) return NotFound();
            s.Actualizar(r.Nombre, r.Direccion, r.Telefono, r.Email ?? "");
            if (!s.IsValid) return BadRequest(s.GetErrors().Select(e => e.ErrorMessage));
            _repo.Update(r.Id, s);
            return NoContent();
        }

        [HttpDelete("api/v1/[Controller]/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _repo.FindOneAsync(id);
            if (s == null) return NotFound();
            s.Desactivar();
            _repo.Update(id, s);
            return NoContent();
        }
    }

    public class CreateSucursalRequest
    {
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
    }

    public class SucursalDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activa { get; set; }
    }
}
