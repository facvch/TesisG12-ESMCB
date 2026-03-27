using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller de Webhooks - sistema de eventos para integraciones externas.
    /// Permite registrar URLs que recibirán notificaciones push cuando ocurran eventos.
    /// </summary>
    [ApiController]
    public class WebhookController : BaseController
    {
        // Almacén en memoria de suscripciones y eventos
        private static readonly ConcurrentDictionary<string, WebhookSubscription> _subscriptions = new();
        private static readonly ConcurrentBag<WebhookEvent> _eventLog = new();
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        // ═══════════════════════════════
        //  EVENTOS DISPONIBLES
        // ═══════════════════════════════

        /// <summary>
        /// Lista todos los tipos de eventos disponibles para suscripción
        /// </summary>
        [HttpGet("api/v1/Webhook/eventos")]
        public IActionResult GetEventosDisponibles()
        {
            var eventos = new[]
            {
                new { Tipo = "turno.creado", Descripcion = "Cuando se programa un nuevo turno", Payload = "{ turnoId, paciente, veterinario, fecha, servicio }" },
                new { Tipo = "turno.completado", Descripcion = "Cuando se marca un turno como completado", Payload = "{ turnoId, paciente, veterinario }" },
                new { Tipo = "turno.cancelado", Descripcion = "Cuando se cancela un turno", Payload = "{ turnoId, paciente, motivo }" },
                new { Tipo = "venta.confirmada", Descripcion = "Cuando se confirma una venta", Payload = "{ ventaId, propietario, total, items }" },
                new { Tipo = "stock.bajo", Descripcion = "Cuando un producto baja del stock mínimo", Payload = "{ productoId, nombre, stockActual, stockMinimo }" },
                new { Tipo = "paciente.creado", Descripcion = "Cuando se registra un nuevo paciente", Payload = "{ pacienteId, nombre, especie, propietario }" },
                new { Tipo = "vacunacion.aplicada", Descripcion = "Cuando se aplica una vacuna", Payload = "{ pacienteId, vacuna, fecha, proximaDosis }" },
                new { Tipo = "backup.creado", Descripcion = "Cuando se crea un backup exitoso", Payload = "{ archivo, tamaño, fecha }" },
            };

            return Ok(new { Total = eventos.Length, Eventos = eventos });
        }

        // ═══════════════════════════════
        //  SUSCRIPCIONES
        // ═══════════════════════════════

        /// <summary>
        /// Registra una nueva suscripción a webhook
        /// </summary>
        [HttpPost("api/v1/Webhook/suscribir")]
        public IActionResult Suscribir([FromBody] WebhookSubscriptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Url))
                return BadRequest("La URL es obligatoria");
            if (request.Eventos == null || request.Eventos.Length == 0)
                return BadRequest("Debe especificar al menos un evento");
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                return BadRequest("URL inválida");

            var id = Guid.NewGuid().ToString("N")[..12];
            var sub = new WebhookSubscription
            {
                Id = id,
                Url = request.Url,
                Eventos = request.Eventos,
                Descripcion = request.Descripcion ?? "",
                Secret = Guid.NewGuid().ToString("N"),
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _subscriptions[id] = sub;

            return Ok(new
            {
                Mensaje = "✅ Webhook suscrito exitosamente",
                sub.Id,
                sub.Url,
                sub.Eventos,
                sub.Secret,
                Nota = "Guarde el Secret. Se enviará como header 'X-Webhook-Secret' en cada notificación."
            });
        }

        /// <summary>
        /// Lista todas las suscripciones activas
        /// </summary>
        [HttpGet("api/v1/Webhook/suscripciones")]
        public IActionResult ListarSuscripciones()
        {
            var subs = _subscriptions.Values
                .OrderByDescending(s => s.FechaCreacion)
                .Select(s => new
                {
                    s.Id, s.Url, s.Eventos, s.Descripcion, s.Activo,
                    s.FechaCreacion, s.UltimaNotificacion,
                    EventosEnviados = s.EventosEnviados,
                    Errores = s.Errores
                }).ToList();

            return Ok(new { Total = subs.Count, Activas = subs.Count(s => s.Activo), Items = subs });
        }

        /// <summary>
        /// Elimina una suscripción
        /// </summary>
        [HttpDelete("api/v1/Webhook/suscripcion/{id}")]
        public IActionResult EliminarSuscripcion(string id)
        {
            if (!_subscriptions.TryRemove(id, out _))
                return NotFound($"Suscripción '{id}' no encontrada");
            return Ok(new { Mensaje = $"✅ Suscripción '{id}' eliminada" });
        }

        /// <summary>
        /// Activa o desactiva una suscripción
        /// </summary>
        [HttpPut("api/v1/Webhook/suscripcion/{id}/toggle")]
        public IActionResult ToggleSuscripcion(string id)
        {
            if (!_subscriptions.TryGetValue(id, out var sub))
                return NotFound($"Suscripción '{id}' no encontrada");
            sub.Activo = !sub.Activo;
            return Ok(new { Mensaje = sub.Activo ? "✅ Suscripción activada" : "⏸️ Suscripción pausada", sub.Id, sub.Activo });
        }

        // ═══════════════════════════════
        //  LOG DE EVENTOS
        // ═══════════════════════════════

        /// <summary>
        /// Consulta el historial de eventos disparados
        /// </summary>
        [HttpGet("api/v1/Webhook/log")]
        public IActionResult GetEventLog([FromQuery] int limite = 50)
        {
            var events = _eventLog
                .OrderByDescending(e => e.Timestamp)
                .Take(limite)
                .Select(e => new
                {
                    e.Id, e.Tipo, e.Timestamp,
                    SuscripcionesNotificadas = e.SuscripcionesNotificadas,
                    e.Exitoso, e.Error
                }).ToList();

            return Ok(new { Total = _eventLog.Count, Items = events });
        }

        // ═══════════════════════════════
        //  DISPARAR EVENTO DE PRUEBA
        // ═══════════════════════════════

        /// <summary>
        /// Dispara un evento de prueba para verificar la integración
        /// </summary>
        [HttpPost("api/v1/Webhook/test")]
        public async Task<IActionResult> TestWebhook([FromBody] WebhookTestRequest request)
        {
            var tipoEvento = request?.TipoEvento ?? "test.ping";
            var payload = new
            {
                Evento = tipoEvento,
                Timestamp = DateTime.Now,
                Datos = new
                {
                    Mensaje = "Este es un evento de prueba",
                    Origen = "API Veterinaria",
                    Version = "v1"
                }
            };

            var result = await DispatchEvent(tipoEvento, payload);
            return Ok(result);
        }

        // ═══════════════════════════════
        //  DISPATCH (API estática para otros controllers)
        // ═══════════════════════════════

        /// <summary>
        /// Envía un evento a todas las suscripciones relevantes.
        /// Puede ser llamado desde otros controllers.
        /// </summary>
        public static async Task<WebhookDispatchResult> DispatchEvent(string tipoEvento, object payload)
        {
            var eventId = Guid.NewGuid().ToString("N")[..12];
            var result = new WebhookDispatchResult
            {
                EventoId = eventId,
                TipoEvento = tipoEvento,
                Timestamp = DateTime.Now
            };

            // Buscar suscripciones que escuchen este evento (o "*")
            var relevantSubs = _subscriptions.Values
                .Where(s => s.Activo && (s.Eventos.Contains(tipoEvento) || s.Eventos.Contains("*")))
                .ToList();

            result.SuscripcionesNotificadas = relevantSubs.Count;

            foreach (var sub in relevantSubs)
            {
                try
                {
                    var json = JsonSerializer.Serialize(new
                    {
                        EventId = eventId,
                        Type = tipoEvento,
                        Timestamp = DateTime.Now,
                        SubscriptionId = sub.Id,
                        Data = payload
                    });

                    var httpReq = new HttpRequestMessage(HttpMethod.Post, sub.Url)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                    httpReq.Headers.Add("X-Webhook-Secret", sub.Secret);
                    httpReq.Headers.Add("X-Webhook-Event", tipoEvento);
                    httpReq.Headers.Add("X-Webhook-Id", eventId);

                    var response = await _httpClient.SendAsync(httpReq);
                    sub.EventosEnviados++;
                    sub.UltimaNotificacion = DateTime.Now;
                    result.Exitoso = true;
                    result.Detalles.Add($"✅ {sub.Url}: {(int)response.StatusCode}");
                }
                catch (Exception ex)
                {
                    sub.Errores++;
                    result.Detalles.Add($"❌ {sub.Url}: {ex.Message}");
                }
            }

            // Registrar en log
            _eventLog.Add(new WebhookEvent
            {
                Id = eventId,
                Tipo = tipoEvento,
                Timestamp = DateTime.Now,
                SuscripcionesNotificadas = relevantSubs.Count,
                Exitoso = result.Exitoso,
                Error = result.Detalles.Where(d => d.StartsWith("❌")).FirstOrDefault()
            });

            return result;
        }
    }

    // ═══════════════════════════════
    //  DTOs y Modelos
    // ═══════════════════════════════

    public class WebhookSubscription
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string[] Eventos { get; set; }
        public string Descripcion { get; set; }
        public string Secret { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public DateTime? UltimaNotificacion { get; set; }
        public int EventosEnviados { get; set; }
        public int Errores { get; set; }
    }

    public class WebhookSubscriptionRequest
    {
        public string Url { get; set; }
        public string[] Eventos { get; set; }
        public string Descripcion { get; set; }
    }

    public class WebhookTestRequest
    {
        public string TipoEvento { get; set; }
    }

    public class WebhookEvent
    {
        public string Id { get; set; }
        public string Tipo { get; set; }
        public DateTime Timestamp { get; set; }
        public int SuscripcionesNotificadas { get; set; }
        public bool Exitoso { get; set; }
        public string Error { get; set; }
    }

    public class WebhookDispatchResult
    {
        public string EventoId { get; set; }
        public string TipoEvento { get; set; }
        public DateTime Timestamp { get; set; }
        public int SuscripcionesNotificadas { get; set; }
        public bool Exitoso { get; set; }
        public List<string> Detalles { get; set; } = new();
    }
}
