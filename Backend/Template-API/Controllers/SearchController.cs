using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller de búsqueda global y filtros avanzados
    /// </summary>
    [ApiController]
    public class SearchController(
        IPacienteRepository pacienteRepo,
        IPropietarioRepository propietarioRepo,
        IProductoRepository productoRepo,
        IVeterinarioRepository veterinarioRepo,
        ITurnoRepository turnoRepo,
        IVacunaRepository vacunaRepo,
        IServicioRepository servicioRepo) : BaseController
    {
        private readonly IPacienteRepository _pacienteRepo = pacienteRepo;
        private readonly IPropietarioRepository _propietarioRepo = propietarioRepo;
        private readonly IProductoRepository _productoRepo = productoRepo;
        private readonly IVeterinarioRepository _veterinarioRepo = veterinarioRepo;
        private readonly ITurnoRepository _turnoRepo = turnoRepo;
        private readonly IVacunaRepository _vacunaRepo = vacunaRepo;
        private readonly IServicioRepository _servicioRepo = servicioRepo;

        /// <summary>
        /// Búsqueda global - busca en pacientes, propietarios, productos y veterinarios
        /// </summary>
        [HttpGet("api/v1/Search")]
        public async Task<IActionResult> GlobalSearch([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return BadRequest("La búsqueda debe tener al menos 2 caracteres");

            var term = q.ToLower();

            // Buscar pacientes
            var pacientes = (await _pacienteRepo.FindAllAsync())
                .Where(p => p.Nombre.ToLower().Contains(term))
                .Take(10)
                .Select(p => new SearchResultDto
                {
                    Tipo = "Paciente",
                    Id = p.Id,
                    Titulo = p.Nombre,
                    Subtitulo = $"{p.Especie?.Nombre ?? ""} | Dueño: {p.Propietario?.Nombre ?? ""} {p.Propietario?.Apellido ?? ""}",
                    Url = $"/api/v1/Paciente/{p.Id}"
                });

            // Buscar propietarios
            var propietarios = (await _propietarioRepo.FindAllAsync())
                .Where(p => p.Nombre.ToLower().Contains(term) ||
                            p.Apellido.ToLower().Contains(term) ||
                            p.DNI.Contains(term))
                .Take(10)
                .Select(p => new SearchResultDto
                {
                    Tipo = "Propietario",
                    Id = p.Id,
                    Titulo = $"{p.Apellido}, {p.Nombre}",
                    Subtitulo = $"DNI: {p.DNI} | Tel: {p.Telefono}",
                    Url = $"/api/v1/Propietario/{p.Id}"
                });

            // Buscar productos
            var productos = (await _productoRepo.FindAllAsync())
                .Where(p => p.Activo && (p.Nombre.ToLower().Contains(term) ||
                            (p.CodigoBarras ?? "").Contains(term)))
                .Take(10)
                .Select(p => new SearchResultDto
                {
                    Tipo = "Producto",
                    Id = p.Id,
                    Titulo = p.Nombre,
                    Subtitulo = $"Stock: {p.StockActual} | ${p.PrecioVenta}",
                    Url = $"/api/v1/Producto/{p.Id}"
                });

            // Buscar veterinarios
            var veterinarios = (await _veterinarioRepo.FindAllAsync())
                .Where(v => v.Nombre.ToLower().Contains(term) ||
                            v.Apellido.ToLower().Contains(term) ||
                            v.Matricula.Contains(term))
                .Take(5)
                .Select(v => new SearchResultDto
                {
                    Tipo = "Veterinario",
                    Id = v.Id.ToString(),
                    Titulo = $"Dr. {v.Apellido}, {v.Nombre}",
                    Subtitulo = $"Mat: {v.Matricula} | {v.Especialidad}",
                    Url = $"/api/v1/Veterinario/{v.Id}"
                });

            var results = pacientes.Concat(propietarios).Concat(productos).Concat(veterinarios).ToList();
            return Ok(new { Total = results.Count, Resultados = results });
        }

        // ═══════════════════════════════════════════
        // FILTROS AVANZADOS POR ENTIDAD
        // ═══════════════════════════════════════════

        /// <summary>
        /// Buscar pacientes con filtros
        /// </summary>
        [HttpGet("api/v1/Search/pacientes")]
        public async Task<IActionResult> SearchPacientes(
            [FromQuery] string nombre, [FromQuery] int? especieId, [FromQuery] int? razaId,
            [FromQuery] string sexo, [FromQuery] string propietarioId)
        {
            var query = (await _pacienteRepo.FindAllAsync()).AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()));
            if (especieId.HasValue)
                query = query.Where(p => p.EspecieId == especieId.Value);
            if (razaId.HasValue)
                query = query.Where(p => p.RazaId == razaId.Value);
            if (!string.IsNullOrWhiteSpace(sexo))
                query = query.Where(p => p.Sexo == sexo);
            if (!string.IsNullOrWhiteSpace(propietarioId))
                query = query.Where(p => p.PropietarioId == propietarioId);

            var results = query.ToList().Select(p => new PacienteSearchDto
            {
                Id = p.Id, Nombre = p.Nombre, Sexo = p.Sexo,
                FechaNacimiento = p.FechaNacimiento,
                EspecieNombre = p.Especie != null ? p.Especie.Nombre : "",
                RazaNombre = p.Raza != null ? p.Raza.Nombre : "",
                PropietarioNombre = (p.Propietario != null ? p.Propietario.Nombre + " " + p.Propietario.Apellido : "")
            }).Take(50).ToList();

            return Ok(results);
        }

        /// <summary>
        /// Buscar propietarios con filtros
        /// </summary>
        [HttpGet("api/v1/Search/propietarios")]
        public async Task<IActionResult> SearchPropietarios(
            [FromQuery] string nombre, [FromQuery] string dni, [FromQuery] string telefono)
        {
            var query = (await _propietarioRepo.FindAllAsync()).AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()) ||
                                         p.Apellido.ToLower().Contains(nombre.ToLower()));
            if (!string.IsNullOrWhiteSpace(dni))
                query = query.Where(p => p.DNI.Contains(dni));
            if (!string.IsNullOrWhiteSpace(telefono))
                query = query.Where(p => p.Telefono.Contains(telefono));

            var results = query.Select(p => new
            {
                p.Id, p.Nombre, p.Apellido, p.DNI, p.Telefono, p.Email, p.Direccion
            }).Take(50).ToList();

            return Ok(results);
        }

        /// <summary>
        /// Buscar productos con filtros
        /// </summary>
        [HttpGet("api/v1/Search/productos")]
        public async Task<IActionResult> SearchProductos(
            [FromQuery] string nombre, [FromQuery] int? categoriaId, [FromQuery] int? marcaId,
            [FromQuery] bool? stockBajo, [FromQuery] decimal? precioMin, [FromQuery] decimal? precioMax)
        {
            var query = (await _productoRepo.FindAllAsync()).Where(p => p.Activo).AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()));
            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            if (marcaId.HasValue)
                query = query.Where(p => p.MarcaId == marcaId.Value);
            if (stockBajo == true)
                query = query.Where(p => p.StockActual <= p.StockMinimo);
            if (precioMin.HasValue)
                query = query.Where(p => p.PrecioVenta >= precioMin.Value);
            if (precioMax.HasValue)
                query = query.Where(p => p.PrecioVenta <= precioMax.Value);

            var results = query.ToList().Select(p => new
            {
                p.Id, p.Nombre, p.CodigoBarras, p.PrecioVenta, p.StockActual, p.StockMinimo,
                CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : "",
                MarcaNombre = p.Marca != null ? p.Marca.Nombre : ""
            }).Take(50).ToList();

            return Ok(results);
        }

        /// <summary>
        /// Buscar turnos con filtros
        /// </summary>
        [HttpGet("api/v1/Search/turnos")]
        public async Task<IActionResult> SearchTurnos(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] string veterinarioId, [FromQuery] int? servicioId,
            [FromQuery] int? estado)
        {
            var d = desde ?? DateTime.Today;
            var h = hasta ?? DateTime.Today.AddDays(30);
            var query = (await _turnoRepo.FindAllAsync())
                .Where(t => t.FechaHora >= d && t.FechaHora <= h).AsQueryable();

            if (!string.IsNullOrWhiteSpace(veterinarioId))
                query = query.Where(t => t.VeterinarioId == veterinarioId);
            if (servicioId.HasValue)
                query = query.Where(t => t.ServicioId == servicioId.Value);
            if (estado.HasValue)
                query = query.Where(t => (int)t.Estado == estado.Value);

            var results = query.OrderBy(t => t.FechaHora).ToList().Select(t => new
            {
                t.Id, t.FechaHora, Estado = t.Estado.ToString(),
                t.PacienteId, PacienteNombre = t.Paciente != null ? t.Paciente.Nombre : "",
                VeterinarioNombre = t.Veterinario != null ? $"Dr. {t.Veterinario.Apellido}" : "",
                ServicioNombre = t.Servicio != null ? t.Servicio.Nombre : "", t.Motivo
            }).Take(100).ToList();

            return Ok(results);
        }
    }

    public class SearchResultDto
    {
        public string Tipo { get; set; }
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public string Url { get; set; }
    }

    public class PacienteSearchDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string EspecieNombre { get; set; }
        public string RazaNombre { get; set; }
        public string PropietarioNombre { get; set; }
    }
}
