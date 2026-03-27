using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace API.Middleware
{
    /// <summary>
    /// Middleware de caché en memoria para respuestas GET.
    /// Cache automático con TTL por categoría y invalidación en escritura.
    /// </summary>
    public class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseCacheMiddleware> _logger;

        // Cache en memoria: key -> (body, contentType, etag, expiry)
        private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private static DateTime _lastCleanup = DateTime.UtcNow;

        public ResponseCacheMiddleware(RequestDelegate next, ILogger<ResponseCacheMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            var method = context.Request.Method;

            // Solo cachear GETs
            if (method != "GET")
            {
                // POST/PUT/DELETE → invalidar cache relevante
                if (method is "POST" or "PUT" or "DELETE")
                    InvalidateCache(path);

                await _next(context);
                return;
            }

            // Determinar si esta ruta es cacheable y su TTL
            var cacheConfig = GetCacheConfig(path);
            if (cacheConfig == null)
            {
                await _next(context);
                return;
            }

            var now = DateTime.UtcNow;

            // Limpieza periódica
            if ((now - _lastCleanup).TotalMinutes > 2)
            {
                CleanupExpired(now);
                _lastCleanup = now;
            }

            // Generar cache key (path + query string)
            var cacheKey = $"{path}?{context.Request.QueryString}";

            // Check If-None-Match (ETag)
            var ifNoneMatch = context.Request.Headers["If-None-Match"].FirstOrDefault();

            // Buscar en cache
            if (_cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > now)
            {
                // Si el cliente ya tiene la misma versión
                if (ifNoneMatch != null && ifNoneMatch == cached.ETag)
                {
                    context.Response.StatusCode = 304; // Not Modified
                    context.Response.Headers["ETag"] = cached.ETag;
                    context.Response.Headers["X-Cache"] = "HIT-304";
                    return;
                }

                // Servir desde cache
                context.Response.StatusCode = 200;
                context.Response.ContentType = cached.ContentType;
                context.Response.Headers["ETag"] = cached.ETag;
                context.Response.Headers["X-Cache"] = "HIT";
                context.Response.Headers["X-Cache-TTL"] = $"{(int)(cached.Expiry - now).TotalSeconds}s";
                context.Response.Headers["Cache-Control"] = $"public, max-age={cacheConfig.TtlSeconds}";
                await context.Response.WriteAsync(cached.Body);
                return;
            }

            // Cache MISS — ejecutar request real y capturar respuesta
            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            memStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();

            // Solo cachear respuestas 200
            if (context.Response.StatusCode == 200 && !string.IsNullOrEmpty(responseBody))
            {
                var etag = $"\"{GenerateETag(responseBody)}\"";
                _cache[cacheKey] = new CacheEntry
                {
                    Body = responseBody,
                    ContentType = context.Response.ContentType ?? "application/json",
                    ETag = etag,
                    Expiry = now.AddSeconds(cacheConfig.TtlSeconds),
                    Category = cacheConfig.Category
                };

                context.Response.Headers["ETag"] = etag;
                context.Response.Headers["X-Cache"] = "MISS";
                context.Response.Headers["Cache-Control"] = $"public, max-age={cacheConfig.TtlSeconds}";
            }

            // Escribir al stream original
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }

        private static CacheConfig GetCacheConfig(string path)
        {
            // Datos estáticos (cambian muy poco): 5 minutos
            if (path.Contains("/especie") && !path.Contains("/paginado"))
                return new CacheConfig("Especie", 300);
            if (path.Contains("/raza") && !path.Contains("/paginado"))
                return new CacheConfig("Raza", 300);
            if (path.Contains("/servicio") && !path.Contains("/paginado"))
                return new CacheConfig("Servicio", 300);
            if (path.Contains("/vacuna"))
                return new CacheConfig("Vacuna", 300);
            if (path.Contains("/metodopago"))
                return new CacheConfig("MetodoPago", 300);
            if (path.Contains("/categoria"))
                return new CacheConfig("Categoria", 300);
            if (path.Contains("/marca"))
                return new CacheConfig("Marca", 300);
            if (path.Contains("/deposito"))
                return new CacheConfig("Deposito", 300);

            // Estadísticas: 1 minuto
            if (path.Contains("/estadisticas/"))
                return new CacheConfig("Estadisticas", 60);

            // Documentación: 10 minutos
            if (path.Contains("/documentacion/"))
                return new CacheConfig("Documentacion", 600);

            // Health check: 30 segundos
            if (path.Contains("/health"))
                return new CacheConfig("Health", 30);

            // No cachear el resto
            return null;
        }

        private void InvalidateCache(string path)
        {
            // Determinar qué categoría invalidar
            string category = null;
            if (path.Contains("/especie")) category = "Especie";
            else if (path.Contains("/raza")) category = "Raza";
            else if (path.Contains("/servicio")) category = "Servicio";
            else if (path.Contains("/vacuna")) category = "Vacuna";
            else if (path.Contains("/metodopago")) category = "MetodoPago";
            else if (path.Contains("/categoria")) category = "Categoria";
            else if (path.Contains("/marca")) category = "Marca";
            else if (path.Contains("/deposito")) category = "Deposito";

            if (category == null) return;

            var keysToRemove = _cache
                .Where(kvp => kvp.Value.Category == category)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
                _cache.TryRemove(key, out _);

            if (keysToRemove.Count > 0)
                _logger.LogInformation("Cache invalidado: {Category} ({Count} entradas)", category, keysToRemove.Count);
        }

        private static void CleanupExpired(DateTime now)
        {
            var expired = _cache.Where(kvp => kvp.Value.Expiry <= now).Select(kvp => kvp.Key).ToList();
            foreach (var key in expired)
                _cache.TryRemove(key, out _);
        }

        private static string GenerateETag(string content)
        {
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(content));
            return Convert.ToHexString(hash).ToLower();
        }

        // API pública para gestión
        public static CacheStats GetStats()
        {
            var entries = _cache.ToArray();
            var now = DateTime.UtcNow;
            return new CacheStats
            {
                TotalEntries = entries.Length,
                ActiveEntries = entries.Count(e => e.Value.Expiry > now),
                ExpiredEntries = entries.Count(e => e.Value.Expiry <= now),
                Categories = entries
                    .Where(e => e.Value.Expiry > now)
                    .GroupBy(e => e.Value.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TotalSizeBytes = entries.Sum(e => (long)Encoding.UTF8.GetByteCount(e.Value.Body))
            };
        }

        public static void ClearAll()
        {
            _cache.Clear();
        }
    }

    internal class CacheEntry
    {
        public string Body { get; set; }
        public string ContentType { get; set; }
        public string ETag { get; set; }
        public DateTime Expiry { get; set; }
        public string Category { get; set; }
    }

    internal record CacheConfig(string Category, int TtlSeconds);

    public class CacheStats
    {
        public int TotalEntries { get; set; }
        public int ActiveEntries { get; set; }
        public int ExpiredEntries { get; set; }
        public Dictionary<string, int> Categories { get; set; }
        public long TotalSizeBytes { get; set; }
    }
}
