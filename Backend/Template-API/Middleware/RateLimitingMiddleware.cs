using System.Collections.Concurrent;
using System.Text.Json;

namespace API.Middleware
{
    /// <summary>
    /// Middleware de Rate Limiting por IP con ventana deslizante.
    /// Aplica límites configurables por patrón de ruta.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitOptions _options;

        // Almacén en memoria de requests por IP
        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();

        // Limpieza periódica
        private static DateTime _lastCleanup = DateTime.UtcNow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _options = new RateLimitOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // No limitar en desarrollo (opcional, se puede comentar)
            // if (env.IsDevelopment()) { await _next(context); return; }

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.ToString().ToLower();
            var now = DateTime.UtcNow;

            // Limpieza periódica cada 5 minutos
            if ((now - _lastCleanup).TotalMinutes > 5)
            {
                CleanupExpiredEntries(now);
                _lastCleanup = now;
            }

            // Determinar límite según la ruta
            var limit = GetLimitForPath(path);
            var windowSeconds = limit.WindowSeconds;
            var maxRequests = limit.MaxRequests;
            var key = $"{ip}:{limit.Category}";

            var clientInfo = _clients.GetOrAdd(key, _ => new ClientRequestInfo());

            lock (clientInfo)
            {
                // Limpiar requests fuera de la ventana
                clientInfo.Requests.RemoveAll(r => (now - r).TotalSeconds > windowSeconds);

                // Verificar límite
                if (clientInfo.Requests.Count >= maxRequests)
                {
                    var oldestInWindow = clientInfo.Requests.Min();
                    var retryAfter = (int)Math.Ceiling(windowSeconds - (now - oldestInWindow).TotalSeconds);
                    if (retryAfter < 1) retryAfter = 1;

                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.Headers["Retry-After"] = retryAfter.ToString();
                    context.Response.Headers["X-RateLimit-Limit"] = maxRequests.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = "0";
                    context.Response.Headers["X-RateLimit-Reset"] = retryAfter.ToString();
                    context.Response.ContentType = "application/json";

                    _logger.LogWarning("Rate limit excedido para IP {Ip} en {Path} ({Category})", ip, path, limit.Category);

                    var response = JsonSerializer.Serialize(new
                    {
                        StatusCode = 429,
                        Message = "Demasiadas solicitudes. Intente de nuevo más tarde.",
                        RetryAfterSeconds = retryAfter,
                        Categoria = limit.Category
                    });

                    context.Response.WriteAsync(response);
                    return;
                }

                // Registrar la request
                clientInfo.Requests.Add(now);
                var remaining = maxRequests - clientInfo.Requests.Count;

                // Agregar headers informativos
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-RateLimit-Limit"] = maxRequests.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                    context.Response.Headers["X-RateLimit-Window"] = $"{windowSeconds}s";
                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }

        private RateLimitRule GetLimitForPath(string path)
        {
            // Auth endpoints: más restrictivos (prevenir brute force)
            if (path.Contains("/auth/login") || path.Contains("/auth/registro"))
                return new RateLimitRule("Auth", 10, 60); // 10 req/min

            // Seed/Backup: muy restrictivos
            if (path.Contains("/seed/") || path.Contains("/backup/crear"))
                return new RateLimitRule("Admin", 5, 60); // 5 req/min

            // Export: moderado (generan carga)
            if (path.Contains("/export/"))
                return new RateLimitRule("Export", 20, 60); // 20 req/min

            // Búsqueda: moderado
            if (path.Contains("/busqueda/"))
                return new RateLimitRule("Busqueda", 30, 60); // 30 req/min

            // Estadísticas: moderado
            if (path.Contains("/estadisticas/"))
                return new RateLimitRule("Estadisticas", 30, 60); // 30 req/min

            // Escritura (POST/PUT/DELETE): moderado
            // Se aplica el default general

            // Default: 100 req/min para GET normales
            return new RateLimitRule("General", 100, 60);
        }

        private static void CleanupExpiredEntries(DateTime now)
        {
            var keysToRemove = _clients
                .Where(kvp =>
                {
                    lock (kvp.Value)
                    {
                        return kvp.Value.Requests.All(r => (now - r).TotalMinutes > 5);
                    }
                })
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
                _clients.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Info de requests de un cliente
    /// </summary>
    internal class ClientRequestInfo
    {
        public List<DateTime> Requests { get; } = new();
    }

    /// <summary>
    /// Regla de rate limit por categoría
    /// </summary>
    internal record RateLimitRule(string Category, int MaxRequests, int WindowSeconds);

    /// <summary>
    /// Opciones de configuración de Rate Limiting
    /// </summary>
    public class RateLimitOptions
    {
        public bool Enabled { get; set; } = true;
        public int DefaultMaxRequests { get; set; } = 100;
        public int DefaultWindowSeconds { get; set; } = 60;
    }
}
