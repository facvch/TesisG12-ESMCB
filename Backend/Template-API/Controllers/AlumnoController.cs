using Application.UseCases.Alumno.Commands.CrearAlumno;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    
    [ApiController]
    public class AlumnoController : BaseController
    {
        private readonly ICommandQueryBus _commandQueryBus;
        
        public AlumnoController(ICommandQueryBus commandQueryBus)
        {
            _commandQueryBus = commandQueryBus ?? throw new ArgumentNullException(nameof(commandQueryBus));
        }

        [HttpGet("api/v1/[controller]")]
        public async Task<IActionResult> GetAlumnos()
        {
            var alumnos = new List<Alumno>
            {
                new Alumno("García", "María Elena", "12345678"),
                new Alumno("Rodríguez", "Juan Carlos", "23456789"),
                new Alumno("Lopez", "Ana Sofía", "34567890"),
                new Alumno("Martínez", "Pedro Luis", "45678901"),
                new Alumno("Fernández", "Laura Isabel", "56789012"),
            };

            return Ok(alumnos);       
        }

        [HttpPost("api/v1/[controller]")]
        public async Task<IActionResult> Create(CrearAlumnoCommand command) 
        {
            if (command is null) return BadRequest();

            var id = await _commandQueryBus.Send(command);

            return Created($"api/[Controller]/{id}", new { Id = id });
        }
    }
}
