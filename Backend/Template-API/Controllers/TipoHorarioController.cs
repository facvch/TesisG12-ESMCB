using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    public class TipoHorarioController(ITipoHorarioRepository tipoHorarioRepository) : BaseController
    {
        private readonly ITipoHorarioRepository _repository = tipoHorarioRepository
            ?? throw new ArgumentNullException(nameof(tipoHorarioRepository));

        [HttpGet("api/v1/[Controller]")]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _repository.GetActivosAsync();
            var dtos = entities.Select(t => new TipoHorarioDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                Activo = t.Activo
            }).ToList();

            return Ok(dtos);
        }
    }
}
