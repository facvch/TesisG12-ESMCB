using Application.Repositories;
using Domain.Entities;
using System.Security.Claims;

namespace API.Middleware
{
    /// <summary>
    /// Middleware que registra automáticamente todas las operaciones de escritura (POST/PUT/DELETE)
    /// </summary>
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuditLogRepository auditRepo)
        {
            // Solo auditar operaciones de escritura
            var method = context.Request.Method;
            if (method != "POST" && method != "PUT" && method != "DELETE")
            {
                await _next(context);
                return;
            }

            // Capturar respuesta
            await _next(context);

            try
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = context.User?.FindFirst(ClaimTypes.Name)?.Value;
                var path = context.Request.Path.Value ?? "";
                var entidad = ExtractEntityName(path);
                var entidadId = ExtractEntityId(path);
                var ip = context.Connection.RemoteIpAddress?.ToString();

                var log = new AuditLog(
                    userId, userName, method, entidad, entidadId,
                    $"{method} {path}", ip, context.Response.StatusCode);

                await auditRepo.AddAsync(log);
            }
            catch
            {
                // No interrumpir flujo si falla la auditoría
            }
        }

        private static string ExtractEntityName(string path)
        {
            // "/api/v1/Paciente/123" → "Paciente"
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 3 ? segments[2] : path;
        }

        private static string ExtractEntityId(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 4 ? segments[3] : "";
        }
    }
}
