using API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para gestión del caché de respuestas
    /// </summary>
    [ApiController]
    public class CacheController : BaseController
    {
        /// <summary>
        /// Obtiene estadísticas del caché
        /// </summary>
        [HttpGet("api/v1/Cache/stats")]
        public IActionResult GetStats()
        {
            var stats = ResponseCacheMiddleware.GetStats();
            return Ok(new
            {
                Mensaje = "Estadísticas del caché de respuestas",
                stats.TotalEntries,
                stats.ActiveEntries,
                stats.ExpiredEntries,
                TamañoTotal = stats.TotalSizeBytes < 1024
                    ? $"{stats.TotalSizeBytes} B"
                    : $"{stats.TotalSizeBytes / 1024.0:F1} KB",
                Categorias = stats.Categories
            });
        }

        /// <summary>
        /// Limpia todo el caché
        /// </summary>
        [HttpDelete("api/v1/Cache/limpiar")]
        public IActionResult LimpiarCache()
        {
            ResponseCacheMiddleware.ClearAll();
            return Ok(new { Mensaje = "✅ Caché limpiado exitosamente" });
        }
    }
}
