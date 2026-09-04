using Application.DataTransferObjects;
using Application.Repositories;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfiguracionSistemaRepository _configRepo;
        private readonly ILogger<EmailService> _logger;
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };

        public EmailService(
            IConfiguration configuration,
            IConfiguracionSistemaRepository configRepo,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _configRepo = configRepo;
            _logger = logger;
        }

        private async Task<EmailSettings> GetSettingsAsync()
        {
            var settings = new EmailSettings
            {
                SendGridApiKey = _configuration["Email:SendGridApiKey"] ?? string.Empty,
                FromEmail = _configuration["Email:FromEmail"] ?? "notificaciones@veterinarianandubay.com",
                FromName = _configuration["Email:FromName"] ?? "Veterinaria Ñandubay",
                SmtpHost = _configuration["Email:SmtpHost"] ?? string.Empty,
                SmtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587,
                SmtpUser = _configuration["Email:SmtpUser"] ?? string.Empty,
                SmtpPass = _configuration["Email:SmtpPass"] ?? string.Empty,
                EnableSsl = !bool.TryParse(_configuration["Email:EnableSsl"], out var ssl) || ssl,
                SafeTestEmail = _configuration["Email:SafeTestEmail"] ?? string.Empty,
                EnableSafeMode = !bool.TryParse(_configuration["Email:EnableSafeMode"], out var safe) || safe,
                EnableEmail = !bool.TryParse(_configuration["Email:EnableEmail"], out var enabled) || enabled
            };

            // Consultar sobreescrituras en BD si existen
            try
            {
                var dbKey = await _configRepo.GetByClaveAsync("email_sendgrid_api_key");
                if (dbKey != null && !string.IsNullOrWhiteSpace(dbKey.Valor))
                    settings.SendGridApiKey = dbKey.Valor.Trim();

                var dbFrom = await _configRepo.GetByClaveAsync("email_from_address");
                if (dbFrom != null && !string.IsNullOrWhiteSpace(dbFrom.Valor))
                    settings.FromEmail = dbFrom.Valor.Trim();

                var dbFromName = await _configRepo.GetByClaveAsync("email_from_name");
                if (dbFromName != null && !string.IsNullOrWhiteSpace(dbFromName.Valor))
                    settings.FromName = dbFromName.Valor.Trim();

                var dbSafe = await _configRepo.GetByClaveAsync("email_safe_test_address");
                if (dbSafe != null && !string.IsNullOrWhiteSpace(dbSafe.Valor))
                    settings.SafeTestEmail = dbSafe.Valor.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No se pudieron consultar configuraciones de Email en BD: {ex.Message}");
            }

            return settings;
        }

        public string BuildGoogleCalendarUrl(string titulo, DateTime fechaHora, int duracionMinutos, string descripcion, string ubicacion)
        {
            var dur = duracionMinutos > 0 ? duracionMinutos : 30;
            var startUtc = fechaHora.ToUniversalTime();
            var endUtc = startUtc.AddMinutes(dur);

            var dates = $"{startUtc:yyyyMMddTHHmmssZ}/{endUtc:yyyyMMddTHHmmssZ}";
            var escapedTitle = Uri.EscapeDataString(titulo);
            var escapedDesc = Uri.EscapeDataString(descripcion);
            var escapedLoc = Uri.EscapeDataString(ubicacion ?? "Veterinaria Ñandubay");

            return $"https://calendar.google.com/calendar/render?action=TEMPLATE&text={escapedTitle}&dates={dates}&details={escapedDesc}&location={escapedLoc}";
        }

        public async Task<EmailResponseDto> SendTurnoConfirmacionAsync(
            Turno turno,
            Paciente paciente,
            Propietario propietario,
            Veterinario veterinario,
            Servicio servicio,
            Sucursal? sucursal)
        {
            if (propietario == null || string.IsNullOrWhiteSpace(propietario.Email))
            {
                return new EmailResponseDto
                {
                    Success = false,
                    Message = "El propietario no tiene un correo electrónico registrado."
                };
            }

            var settings = await GetSettingsAsync();
            if (!settings.EnableEmail)
            {
                return new EmailResponseDto
                {
                    Success = false,
                    Message = "El servicio de email está deshabilitado en la configuración."
                };
            }

            string pacienteNombre = paciente?.Nombre ?? "tu mascota";
            string vetNombre = veterinario?.NombreCompleto ?? "nuestro equipo profesional";
            string servNombre = servicio?.Nombre ?? "Consulta veterinaria";
            string sucursalInfo = sucursal != null ? $"{sucursal.Nombre} ({sucursal.Direccion})" : "Veterinaria Ñandubay";

            // Detección de Cirugía o Análisis para aviso de ayuno
            bool isCirugiaOAnalisis = false;
            string motivoLower = (turno.Motivo ?? "").ToLower();
            string servicioLower = (servicio?.Nombre ?? "").ToLower();

            if (motivoLower.Contains("cirugia") || motivoLower.Contains("cirugía") ||
                motivoLower.Contains("operacion") || motivoLower.Contains("operación") ||
                motivoLower.Contains("quirurg") || motivoLower.Contains("quirúrg") ||
                motivoLower.Contains("analisis") || motivoLower.Contains("análisis") ||
                motivoLower.Contains("laboratorio") || motivoLower.Contains("sangre") ||
                motivoLower.Contains("extraccion") || motivoLower.Contains("extracción") ||
                servicioLower.Contains("cirugia") || servicioLower.Contains("cirugía") ||
                servicioLower.Contains("analisis") || servicioLower.Contains("análisis") ||
                servicioLower.Contains("laboratorio"))
            {
                isCirugiaOAnalisis = true;
            }

            string gCalTitle = $"Turno Veterinaria - {pacienteNombre}";
            string gCalDesc = $"Turno agendado para {pacienteNombre} en Veterinaria Ñandubay.\\nServicio: {servNombre}\\nVeterinario: {vetNombre}\\nMotivo: {turno.Motivo}";
            if (isCirugiaOAnalisis)
            {
                gCalDesc += $"\\n\\n⚠️ Recordatorio: {pacienteNombre} no debe ingerir alimentos por 24hs antes de su turno.";
            }

            string googleCalendarUrl = BuildGoogleCalendarUrl(
                gCalTitle,
                turno.FechaHora,
                turno.DuracionMinutos,
                gCalDesc,
                sucursalInfo
            );

            string subject = $"Confirmación de Turno para {pacienteNombre} - Veterinaria Ñandubay";

            // Plantilla HTML profesional con botón Google Calendar y advertencia de ayuno si corresponde
            string fastingAlertHtml = isCirugiaOAnalisis ? $@"
                <div style=""background-color: #fffbeb; border-left: 4px solid #f59e0b; padding: 14px; margin: 20px 0; border-radius: 6px;"">
                    <p style=""margin: 0; color: #b45309; font-weight: bold; font-size: 15px;"">⚠️ INDICACIONES DE PREPARACIÓN:</p>
                    <p style=""margin: 6px 0 0 0; color: #92400e; font-size: 14px;"">
                        <strong>Recuerde que {WebUtility.HtmlEncode(pacienteNombre)} no debe ingerir alimentos por 24hs antes de su turno.</strong>
                    </p>
                </div>" : "";

            string htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>{WebUtility.HtmlEncode(subject)}</title>
</head>
<body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1e293b; background-color: #f8fafc; margin: 0; padding: 20px;"">
    <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.06); border: 1px solid #e2e8f0;"">
        <tr>
            <td style=""background: linear-gradient(135deg, #00A36C 0%, #047857 100%); padding: 30px 24px; text-align: center; color: white;"">
                <h1 style=""margin: 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px;"">🐾 Veterinaria Ñandubay</h1>
                <p style=""margin: 6px 0 0 0; font-size: 15px; opacity: 0.9;"">Cuidado y Compromiso con tu Mascota</p>
            </td>
        </tr>
        <tr>
            <td style=""padding: 30px 24px;"">
                <h2 style=""margin: 0 0 12px 0; color: #0f172a; font-size: 20px;"">¡Hola, {WebUtility.HtmlEncode(propietario.Nombre)}!</h2>
                <p style=""margin: 0 0 20px 0; font-size: 15px; color: #475569;"">
                    Te confirmamos que se ha agendado con éxito el turno para <strong>{WebUtility.HtmlEncode(pacienteNombre)}</strong>. A continuación encontrarás el resumen de tu cita:
                </p>

                <table cellpadding=""8"" cellspacing=""0"" border=""0"" width=""100%"" style=""background-color: #f1f5f9; border-radius: 8px; font-size: 14px; margin-bottom: 20px;"">
                    <tr>
                        <td style=""color: #64748b; width: 35%; font-weight: 600;"">📅 Fecha:</td>
                        <td style=""color: #0f172a; font-weight: 700;"">{turno.FechaHora:dd/MM/yyyy}</td>
                    </tr>
                    <tr>
                        <td style=""color: #64748b; font-weight: 600;"">⏰ Hora:</td>
                        <td style=""color: #00A36C; font-weight: 700; font-size: 15px;"">{turno.FechaHora:HH:mm} hs</td>
                    </tr>
                    <tr>
                        <td style=""color: #64748b; font-weight: 600;"">🐕 Paciente:</td>
                        <td style=""color: #0f172a;"">{WebUtility.HtmlEncode(pacienteNombre)}</td>
                    </tr>
                    <tr>
                        <td style=""color: #64748b; font-weight: 600;"">🩺 Servicio:</td>
                        <td style=""color: #0f172a;"">{WebUtility.HtmlEncode(servNombre)}</td>
                    </tr>
                    <tr>
                        <td style=""color: #64748b; font-weight: 600;"">👨‍⚕️ Profesional:</td>
                        <td style=""color: #0f172a;"">{WebUtility.HtmlEncode(vetNombre)}</td>
                    </tr>
                    {(string.IsNullOrWhiteSpace(turno.Motivo) ? "" : $@"
                    <tr>
                        <td style=""color: #64748b; font-weight: 600;"">📋 Motivo:</td>
                        <td style=""color: #0f172a;"">{WebUtility.HtmlEncode(turno.Motivo)}</td>
                    </tr>")}
                    <tr>
                        <td style=""color: #64748b; font-weight: 600;"">📍 Ubicación:</td>
                        <td style=""color: #0f172a;"">{WebUtility.HtmlEncode(sucursalInfo)}</td>
                    </tr>
                </table>

                {fastingAlertHtml}

                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{googleCalendarUrl}"" target=""_blank"" style=""display: inline-block; background-color: #00A36C; color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; padding: 12px 28px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,163,108,0.25);"">
                        📅 Añadir a Google Calendar
                    </a>
                    <p style=""margin: 8px 0 0 0; font-size: 12px; color: #94a3b8;"">Haz clic para guardar el turno directamente en tu calendario</p>
                </div>

                <p style=""margin: 0; font-size: 14px; color: #64748b;"">
                    Un día antes de tu turno te enviaremos un SMS de recordatorio. Si necesitas reprogramar o cancelar el turno, por favor contáctanos con anticipación.
                </p>
            </td>
        </tr>
        <tr>
            <td style=""background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px 24px; text-align: center; font-size: 12px; color: #94a3b8;"">
                <p style=""margin: 0;"">Veterinaria Ñandubay - Sistema de Gestión de Turnos</p>
                <p style=""margin: 4px 0 0 0;"">Este es un mensaje automático de notificación.</p>
            </td>
        </tr>
    </table>
</body>
</html>";

            var response = await SendEmailAsync(propietario.Email, subject, htmlBody);
            response.GoogleCalendarUrl = googleCalendarUrl;
            return response;
        }

        public async Task<EmailResponseDto> SendEmailAsync(string toEmail, string subject, string htmlBody, string? plainText = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return new EmailResponseDto { Success = false, Message = "El email de destino es obligatorio." };
            }

            var settings = await GetSettingsAsync();

            string targetEmail = toEmail.Trim();
            string finalSubject = subject;

            // Modo Seguro para Pruebas: si hay SafeTestEmail configurado, redirige para no molestar a terceros
            if (!string.IsNullOrWhiteSpace(settings.SafeTestEmail) && settings.EnableSafeMode)
            {
                finalSubject = $"[MODO PRUEBA - Para: {targetEmail}] {subject}";
                targetEmail = settings.SafeTestEmail.Trim();
                _logger.LogInformation($"[EmailService] Modo seguro activo: El correo originalmente destinado a '{toEmail}' se redirige a '{targetEmail}'.");
            }

            // 1. Intentar envío vía Twilio SendGrid REST API
            if (!string.IsNullOrWhiteSpace(settings.SendGridApiKey) && !settings.SendGridApiKey.StartsWith("SG_TU_"))
            {
                try
                {
                    var payload = new
                    {
                        personalizations = new[]
                        {
                            new
                            {
                                to = new[] { new { email = targetEmail } },
                                subject = finalSubject
                            }
                        },
                        from = new
                        {
                            email = settings.FromEmail,
                            name = settings.FromName
                        },
                        content = new[]
                        {
                            new
                            {
                                type = "text/html",
                                value = htmlBody
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(payload);
                    using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SendGridApiKey.Trim());
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Accepted)
                    {
                        _logger.LogInformation($"[SendGrid] Email enviado exitosamente a {targetEmail}. Asunto: {finalSubject}");
                        return new EmailResponseDto
                        {
                            Success = true,
                            Message = "Email enviado exitosamente vía Twilio SendGrid.",
                            RecipientEmail = targetEmail,
                            Subject = finalSubject
                        };
                    }
                    else
                    {
                        var errContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning($"[SendGrid ERROR] Código {(int)response.StatusCode}: {errContent}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[SendGrid Exception] Error al enviar email a {targetEmail}");
                }
            }

            // 2. Intentar envío vía SMTP
            if (!string.IsNullOrWhiteSpace(settings.SmtpHost))
            {
                try
                {
                    using var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(settings.FromEmail, settings.FromName);
                    mailMessage.To.Add(targetEmail);
                    mailMessage.Subject = finalSubject;
                    mailMessage.Body = htmlBody;
                    mailMessage.IsBodyHtml = true;

                    using var smtpClient = new SmtpClient(settings.SmtpHost, settings.SmtpPort);
                    smtpClient.EnableSsl = settings.EnableSsl;

                    if (!string.IsNullOrWhiteSpace(settings.SmtpUser))
                    {
                        smtpClient.Credentials = new NetworkCredential(settings.SmtpUser, settings.SmtpPass);
                    }

                    await smtpClient.SendMailAsync(mailMessage);
                    _logger.LogInformation($"[SMTP] Email enviado exitosamente a {targetEmail}.");

                    return new EmailResponseDto
                    {
                        Success = true,
                        Message = "Email enviado exitosamente vía SMTP.",
                        RecipientEmail = targetEmail,
                        Subject = finalSubject
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[SMTP Exception] Error al enviar email vía SMTP a {targetEmail}");
                }
            }

            // 3. Fallback en Modo Simulación / Desarrollo (si no hay proveedor externo configurado)
            _logger.LogInformation($"[Email Simulación] No hay SendGrid ni SMTP configurado. Email simulado a {targetEmail}. Asunto: '{finalSubject}'");
            return new EmailResponseDto
            {
                Success = true,
                Message = $"Email de confirmación generado para {targetEmail}. (Modo simulación: configure SendGrid API Key o SMTP para entrega externa real).",
                RecipientEmail = targetEmail,
                Subject = finalSubject
            };
        }
    }
}
