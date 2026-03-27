using API.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Dashboard de performance — métricas de tiempo de respuesta y rendimiento
    /// </summary>
    [ApiController]
    public class PerformanceController : BaseController
    {
        /// <summary>
        /// Overview de performance del sistema
        /// </summary>
        [HttpGet("api/v1/Performance/dashboard")]
        public IActionResult Dashboard()
        {
            var summary = PerformanceMiddleware.GetSummary();
            var slow = summary.Endpoints.Where(e => e.AvgMs > 100).OrderByDescending(e => e.AvgMs).Take(5).ToList();
            var hot = summary.Endpoints.OrderByDescending(e => e.Requests).Take(10).ToList();

            return Ok(new
            {
                summary.Uptime,
                summary.TotalRequests,
                summary.RequestsPerSecond,
                PromedioGeneral = $"{summary.AvgResponseMs}ms",
                EndpointsMasLentos = slow.Select(e => new { e.Endpoint, Avg = $"{e.AvgMs}ms", P95 = $"{e.P95Ms}ms", e.Requests }),
                EndpointsMasUsados = hot.Select(e => new { e.Endpoint, e.Requests, Avg = $"{e.AvgMs}ms" })
            });
        }

        /// <summary>
        /// Detalle completo de métricas por endpoint
        /// </summary>
        [HttpGet("api/v1/Performance/endpoints")]
        public IActionResult Endpoints([FromQuery] string ordenarPor = "requests")
        {
            var summary = PerformanceMiddleware.GetSummary();

            var endpoints = ordenarPor.ToLower() switch
            {
                "avg" => summary.Endpoints.OrderByDescending(e => e.AvgMs).ToList(),
                "max" => summary.Endpoints.OrderByDescending(e => e.MaxMs).ToList(),
                "p95" => summary.Endpoints.OrderByDescending(e => e.P95Ms).ToList(),
                _ => summary.Endpoints.OrderByDescending(e => e.Requests).ToList()
            };

            return Ok(new
            {
                summary.Uptime,
                summary.TotalRequests,
                TotalEndpoints = endpoints.Count,
                OrdenadoPor = ordenarPor,
                Items = endpoints.Select(e => new
                {
                    e.Endpoint, e.Requests,
                    Avg = $"{e.AvgMs}ms", Min = $"{e.MinMs}ms", Max = $"{e.MaxMs}ms",
                    P95 = $"{e.P95Ms}ms", P99 = $"{e.P99Ms}ms", Last = $"{e.LastMs}ms"
                })
            });
        }

        /// <summary>
        /// Resetea las métricas de performance
        /// </summary>
        [HttpDelete("api/v1/Performance/reset")]
        public IActionResult Reset()
        {
            PerformanceMiddleware.Reset();
            return Ok(new { Mensaje = "✅ Métricas de performance reseteadas" });
        }
    }
}
