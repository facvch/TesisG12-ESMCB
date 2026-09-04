using Application.DataTransferObjects;
using Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Controllers
{
    [ApiController]
    [Authorize]
    public class WhatsAppController : BaseController
    {
        private readonly ITwilioWhatsAppService _twilioService;
        private readonly ITwilioSmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly ILogger<WhatsAppController> _logger;

        public WhatsAppController(
            ITwilioWhatsAppService twilioService,
            ITwilioSmsService smsService,
            IEmailService emailService,
            ILogger<WhatsAppController> logger)
        {
            _twilioService = twilioService;
            _smsService = smsService;
            _emailService = emailService;
            _logger = logger;
        }

        // ═══════════════════════════════
        //  TWILIO SMS
        // ═══════════════════════════════

        /// <summary>
        /// Obtiene el estado de configuración de Twilio SMS y Email
        /// </summary>
        [HttpGet("api/v1/Sms/status")]
        [HttpGet("api/v1/Notificaciones/status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSmsStatus()
        {
            var status = await _smsService.GetStatusAsync();
            return Ok(status);
        }

        /// <summary>
        /// Envía un SMS directo a través de Twilio
        /// </summary>
        [HttpPost("api/v1/Sms/send")]
        public async Task<IActionResult> SendSms([FromBody] SendSmsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Telefono) || string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return BadRequest("El número de teléfono y el mensaje son campos obligatorios.");
            }

            var result = await _smsService.SendSmsAsync(request.Telefono, request.Mensaje);
            return Ok(result);
        }

        /// <summary>
        /// Realiza una prueba de envío de SMS vía Twilio
        /// </summary>
        [HttpPost("api/v1/Sms/test")]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> SendTestSms([FromBody] SendSmsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Telefono))
            {
                return BadRequest("Debe indicar un número de teléfono para la prueba.");
            }

            string testMsg = string.IsNullOrWhiteSpace(request.Mensaje)
                ? "🐾 Mensaje de prueba SMS desde Veterinaria Ñandubay (Twilio SMS Integration activa)."
                : request.Mensaje;

            var result = await _smsService.SendSmsAsync(request.Telefono, testMsg);
            return Ok(result);
        }

        // ═══════════════════════════════
        //  EMAIL
        // ═══════════════════════════════

        /// <summary>
        /// Envía un email personalizado (HTML o texto plano)
        /// </summary>
        [HttpPost("api/v1/Email/send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ToEmail) || string.IsNullOrWhiteSpace(request.Subject))
            {
                return BadRequest("El email de destino y el asunto son obligatorios.");
            }

            var result = await _emailService.SendEmailAsync(request.ToEmail, request.Subject, request.Body);
            return Ok(result);
        }

        /// <summary>
        /// Realiza una prueba de envío de Email
        /// </summary>
        [HttpPost("api/v1/Email/test")]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> SendTestEmail([FromBody] SendEmailRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return BadRequest("Debe indicar un email para la prueba.");
            }

            string subject = string.IsNullOrWhiteSpace(request.Subject)
                ? "🐾 Prueba de Notificación - Veterinaria Ñandubay"
                : request.Subject;

            string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #00A36C; border-radius: 8px;'>
                    <h2 style='color: #00A36C;'>🐾 Veterinaria Ñandubay</h2>
                    <p>Este es un email de prueba del sistema de notificaciones de turnos.</p>
                    <p>{WebUtility.HtmlEncode(request.Body ?? "El servicio de email se encuentra correctamente vinculado y operativo.")}</p>
                </div>";

            var result = await _emailService.SendEmailAsync(request.ToEmail, subject, htmlBody);
            return Ok(result);
        }

        // ═══════════════════════════════
        //  TWILIO WHATSAPP (COMPATIBILIDAD)
        // ═══════════════════════════════

        /// <summary>
        /// Obtiene el estado de configuración de Twilio WhatsApp
        /// </summary>
        [HttpGet("api/v1/WhatsApp/status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _twilioService.GetStatusAsync();
            return Ok(status);
        }

        /// <summary>
        /// Envía un mensaje de WhatsApp directo a través de Twilio
        /// </summary>
        [HttpPost("api/v1/WhatsApp/send")]
        public async Task<IActionResult> SendMessage([FromBody] SendWhatsAppRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Telefono) || string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return BadRequest("El número de teléfono y el mensaje son campos obligatorios.");
            }

            var result = await _twilioService.SendWhatsAppAsync(request.Telefono, request.Mensaje);
            return Ok(result);
        }

        /// <summary>
        /// Realiza una prueba de envío de WhatsApp (para administradores)
        /// </summary>
        [HttpPost("api/v1/WhatsApp/test")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTestMessage([FromBody] SendWhatsAppRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Telefono))
            {
                return BadRequest("Debe indicar un número de teléfono para la prueba.");
            }

            string testMsg = string.IsNullOrWhiteSpace(request.Mensaje)
                ? "🐾 Mensaje de prueba desde Veterinaria Ñandubay (Twilio WhatsApp Integration activa)."
                : request.Mensaje;

            var result = await _twilioService.SendWhatsAppAsync(request.Telefono, testMsg);
            return Ok(result);
        }
    }
}
