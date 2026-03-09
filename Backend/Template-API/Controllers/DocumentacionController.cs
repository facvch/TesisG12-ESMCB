using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Controllers
{
    /// <summary>
    /// Controller de documentación que genera catálogo completo de la API
    /// </summary>
    [ApiController]
    public class DocumentacionController(IActionDescriptorCollectionProvider actionProvider) : BaseController
    {
        /// <summary>
        /// Genera el catálogo completo de todos los endpoints de la API
        /// </summary>
        [HttpGet("api/v1/Documentacion/endpoints")]
        public IActionResult GetEndpoints()
        {
            var actions = actionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(a => a.ControllerName != "Documentacion")
                .GroupBy(a => a.ControllerName)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Controller = g.Key,
                    Endpoints = g.Select(a => new
                    {
                        Metodo = GetHttpMethod(a),
                        Ruta = a.AttributeRouteInfo?.Template ?? "",
                        Accion = a.ActionName,
                        Parametros = a.Parameters.Select(p => new
                        {
                            p.Name,
                            Tipo = SimplifyType(p.ParameterType),
                            Requerido = !p.ParameterType.IsClass || p.ParameterType == typeof(string)
                        }).ToList()
                    }).OrderBy(e => e.Ruta).ToList()
                }).ToList();

            return Ok(new
            {
                Titulo = "API Veterinaria Clínica - Documentación",
                Version = "v1",
                TotalControllers = actions.Count,
                TotalEndpoints = actions.Sum(c => c.Endpoints.Count),
                GeneradoEn = DateTime.Now,
                Controllers = actions
            });
        }

        /// <summary>
        /// Resumen ejecutivo de la API para documentación de tesis
        /// </summary>
        [HttpGet("api/v1/Documentacion/resumen")]
        public IActionResult GetResumen()
        {
            var actions = actionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .ToList();

            var porController = actions
                .GroupBy(a => a.ControllerName)
                .Select(g => new { Controller = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            var porMetodo = actions
                .Select(a => GetHttpMethod(a))
                .GroupBy(m => m)
                .Select(g => new { Metodo = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            return Ok(new
            {
                Titulo = "Resumen de la API - Sistema de Gestión Veterinaria",
                Descripcion = "API RESTful desarrollada con ASP.NET Core 8.0 siguiendo arquitectura DDD híbrida",
                TotalEndpoints = actions.Count,
                TotalControllers = porController.Count,
                EndpointsPorController = porController,
                EndpointsPorMetodoHTTP = porMetodo,
                Tecnologias = new[]
                {
                    "ASP.NET Core 8.0",
                    "Entity Framework Core (SQLite)",
                    "JWT Authentication",
                    "Swagger/OpenAPI",
                    "Domain-Driven Design (DDD)"
                },
                Modulos = new[]
                {
                    new { Nombre = "Gestión de Pacientes", Desc = "CRUD pacientes, especies, razas, propietarios" },
                    new { Nombre = "Historial Clínico", Desc = "Consultas, vacunaciones, tratamientos" },
                    new { Nombre = "Turnos", Desc = "Agenda, superposición, estados de flujo" },
                    new { Nombre = "Stock y Productos", Desc = "Inventario, movimientos, alertas de stock" },
                    new { Nombre = "Ventas y Facturación", Desc = "Ventas, detalles, métodos de pago" },
                    new { Nombre = "Autenticación", Desc = "JWT, roles, usuarios" },
                    new { Nombre = "Estadísticas", Desc = "KPIs, tendencias, métricas" },
                    new { Nombre = "Exportación", Desc = "CSV para todas las entidades" },
                    new { Nombre = "Búsqueda", Desc = "Global y filtrada por entidad" },
                    new { Nombre = "Paginación", Desc = "Paginado y ordenamiento dinámico" },
                    new { Nombre = "Recordatorios", Desc = "Alertas automáticas y dashboard" },
                    new { Nombre = "Integridad", Desc = "Health check, validación de dependencias" },
                    new { Nombre = "Configuración", Desc = "Parámetros del sistema" },
                    new { Nombre = "Auditoría", Desc = "Logs de operaciones" },
                    new { Nombre = "Seed", Desc = "Datos de ejemplo para demostraciones" }
                }
            });
        }

        /// <summary>
        /// Genera documentación en formato Markdown
        /// </summary>
        [HttpGet("api/v1/Documentacion/markdown")]
        public IActionResult GetMarkdown()
        {
            var actions = actionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(a => a.ControllerName != "Documentacion")
                .GroupBy(a => a.ControllerName)
                .OrderBy(g => g.Key);

            var md = "# API Veterinaria Clínica - Documentación\n\n";
            md += $"**Generado:** {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";
            md += "---\n\n";

            foreach (var group in actions)
            {
                md += $"## {group.Key}\n\n";
                md += "| Método | Ruta | Acción | Parámetros |\n";
                md += "|--------|------|--------|------------|\n";

                foreach (var action in group.OrderBy(a => a.AttributeRouteInfo?.Template))
                {
                    var method = GetHttpMethod(action);
                    var route = action.AttributeRouteInfo?.Template ?? "";
                    var parms = string.Join(", ", action.Parameters.Select(p => $"{p.Name}:{SimplifyType(p.ParameterType)}"));
                    md += $"| `{method}` | `{route}` | {action.ActionName} | {parms} |\n";
                }
                md += "\n";
            }

            return Content(md, "text/markdown");
        }

        private static string GetHttpMethod(ControllerActionDescriptor action)
        {
            var attrs = action.MethodInfo.GetCustomAttributes(true);
            if (attrs.Any(a => a is HttpPostAttribute)) return "POST";
            if (attrs.Any(a => a is HttpPutAttribute)) return "PUT";
            if (attrs.Any(a => a is HttpDeleteAttribute)) return "DELETE";
            if (attrs.Any(a => a is HttpPatchAttribute)) return "PATCH";
            return "GET";
        }

        private static string SimplifyType(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int) || type == typeof(int?)) return "int";
            if (type == typeof(bool) || type == typeof(bool?)) return "bool";
            if (type == typeof(decimal) || type == typeof(decimal?)) return "decimal";
            if (type == typeof(DateTime) || type == typeof(DateTime?)) return "DateTime";
            if (type == typeof(uint)) return "uint";
            return type.Name;
        }
    }
}
