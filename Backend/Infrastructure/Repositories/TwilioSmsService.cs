using Application.DataTransferObjects;
using Application.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public class TwilioSmsService : ITwilioSmsService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfiguracionSistemaRepository _configRepo;
        private readonly ILogger<TwilioSmsService> _logger;
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };

        public TwilioSmsService(
            IConfiguration configuration,
            IConfiguracionSistemaRepository configRepo,
            ILogger<TwilioSmsService> logger)
        {
            _configuration = configuration;
            _configRepo = configRepo;
            _logger = logger;
        }

        private async Task<TwilioSettings> GetSettingsAsync()
        {
            var settings = new TwilioSettings
            {
                AccountSid = _configuration["Twilio:AccountSid"] ?? string.Empty,
                AuthToken = _configuration["Twilio:AuthToken"] ?? string.Empty,
                FromPhoneNumber = _configuration["Twilio:FromPhoneNumber"] ?? "+17372508034",
                SmsFromPhoneNumber = _configuration["Twilio:SmsFromPhoneNumber"] ?? "+17372508034",
                DefaultCountryCode = _configuration["Twilio:DefaultCountryCode"] ?? "+549",
                EnableTwilio = bool.TryParse(_configuration["Twilio:EnableTwilio"], out var enabled) ? enabled : true,
                EnableAutomaticDailySms = bool.TryParse(_configuration["Twilio:EnableAutomaticDailySms"], out var autoSms) ? autoSms : true
            };

            // Intentar enriquecer o sobreescribir desde ConfiguracionSistema en BD si existen
            try
            {
                var dbSid = await _configRepo.GetByClaveAsync("twilio_account_sid");
                if (dbSid != null && !string.IsNullOrWhiteSpace(dbSid.Valor))
                    settings.AccountSid = dbSid.Valor.Trim();

                var dbToken = await _configRepo.GetByClaveAsync("twilio_auth_token");
                if (dbToken != null && !string.IsNullOrWhiteSpace(dbToken.Valor))
                    settings.AuthToken = dbToken.Valor.Trim();

                var dbSmsFrom = await _configRepo.GetByClaveAsync("twilio_sms_from_phone");
                if (dbSmsFrom != null && !string.IsNullOrWhiteSpace(dbSmsFrom.Valor))
                    settings.SmsFromPhoneNumber = dbSmsFrom.Valor.Trim();
                else
                {
                    var dbFrom = await _configRepo.GetByClaveAsync("twilio_from_phone");
                    if (dbFrom != null && !string.IsNullOrWhiteSpace(dbFrom.Valor))
                    {
                        var cleanFrom = dbFrom.Valor.Replace("whatsapp:", "").Trim();
                        if (!string.IsNullOrEmpty(cleanFrom))
                            settings.SmsFromPhoneNumber = cleanFrom;
                    }
                }

                var dbCode = await _configRepo.GetByClaveAsync("twilio_default_country_code");
                if (dbCode != null && !string.IsNullOrWhiteSpace(dbCode.Valor))
                    settings.DefaultCountryCode = dbCode.Valor.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No se pudieron consultar configuraciones de Twilio SMS en BD: {ex.Message}");
            }

            return settings;
        }

        public async Task<TwilioStatusDto> GetStatusAsync()
        {
            var settings = await GetSettingsAsync();
            bool isConfigured = !string.IsNullOrWhiteSpace(settings.AccountSid) &&
                                !string.IsNullOrWhiteSpace(settings.AuthToken) &&
                                !settings.AccountSid.StartsWith("AC_TU_") &&
                                !settings.AccountSid.Contains("YOUR_TWILIO");

            string safeEmail = _configuration["Email:SafeTestEmail"] ?? "";

            return new TwilioStatusDto
            {
                IsConfigured = isConfigured,
                EnableTwilio = settings.EnableTwilio,
                FromPhoneNumber = settings.FromPhoneNumber,
                SmsFromPhoneNumber = settings.SmsFromPhoneNumber,
                DefaultCountryCode = settings.DefaultCountryCode,
                EmailConfigured = !string.IsNullOrWhiteSpace(_configuration["Email:SendGridApiKey"]) ||
                                  !string.IsNullOrWhiteSpace(_configuration["Email:SmtpHost"]),
                SafeTestEmail = safeEmail
            };
        }

        public async Task<SmsResponseDto> SendSmsAsync(string telefono, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return new SmsResponseDto
                {
                    Success = false,
                    Message = "El número de teléfono destinatario no puede estar vacío."
                };
            }

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return new SmsResponseDto
                {
                    Success = false,
                    Message = "El mensaje a enviar no puede estar vacío."
                };
            }

            var settings = await GetSettingsAsync();

            if (!settings.EnableTwilio)
            {
                return new SmsResponseDto
                {
                    Success = false,
                    Message = "El servicio de Twilio SMS se encuentra deshabilitado en la configuración."
                };
            }

            if (string.IsNullOrWhiteSpace(settings.AccountSid) || string.IsNullOrWhiteSpace(settings.AuthToken) ||
                settings.AccountSid.StartsWith("AC_TU_") || settings.AccountSid.Contains("YOUR_TWILIO"))
            {
                return new SmsResponseDto
                {
                    Success = false,
                    Message = "Twilio SMS no está configurado con credenciales válidas (Account SID o Auth Token). Verifique appsettings.json."
                };
            }

            string formattedTo = FormatSmsRecipient(telefono, settings.DefaultCountryCode);
            string formattedFrom = CleanPhoneNumber(settings.SmsFromPhoneNumber);
            if (string.IsNullOrWhiteSpace(formattedFrom))
            {
                formattedFrom = CleanPhoneNumber(settings.FromPhoneNumber);
            }

            if (!formattedFrom.StartsWith("+"))
            {
                formattedFrom = "+" + formattedFrom;
            }

            try
            {
                var requestUrl = $"https://api.twilio.com/2010-04-01/Accounts/{settings.AccountSid}/Messages.json";

                var formParams = new Dictionary<string, string>
                {
                    { "From", formattedFrom },
                    { "To", formattedTo },
                    { "Body", mensaje }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                var authBytes = Encoding.ASCII.GetBytes($"{settings.AccountSid}:{settings.AuthToken}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                request.Content = new FormUrlEncodedContent(formParams);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;
                    string sid = root.TryGetProperty("sid", out var sidProp) ? sidProp.GetString() ?? "" : "";
                    string status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "" : "";

                    _logger.LogInformation($"[Twilio SMS] Mensaje enviado exitosamente a {formattedTo}. SID: {sid}, Estado: {status}");

                    return new SmsResponseDto
                    {
                        Success = true,
                        Message = "Recordatorio enviado exitosamente vía Twilio SMS.",
                        MessageSid = sid,
                        FormattedPhone = formattedTo,
                        Status = status
                    };
                }
                else
                {
                    string errorMsg = "Error al enviar SMS vía Twilio.";
                    int errorCode = 0;
                    try
                    {
                        using var doc = JsonDocument.Parse(responseContent);
                        if (doc.RootElement.TryGetProperty("message", out var msgProp))
                        {
                            errorMsg = msgProp.GetString() ?? errorMsg;
                        }
                        if (doc.RootElement.TryGetProperty("code", out var codeProp))
                        {
                            errorCode = codeProp.GetInt32();
                        }
                    }
                    catch { }

                    _logger.LogWarning($"[Twilio SMS ERROR] Código {(int)response.StatusCode} (Twilio Code: {errorCode}) al enviar a {formattedTo}: {errorMsg}");

                    string userFriendlyMsg;
                    if (errorCode == 21608 || errorMsg.Contains("unverified", StringComparison.OrdinalIgnoreCase))
                    {
                        userFriendlyMsg = $"Twilio Trial: El número {formattedTo} no está verificado en tu consola de Twilio (Verified Caller IDs). En modo de prueba solo se pueden enviar SMS a números verificados.";
                    }
                    else if (errorCode == 21211 || errorMsg.Contains("invalid 'To' Phone Number", StringComparison.OrdinalIgnoreCase))
                    {
                        userFriendlyMsg = $"El número destinatario {formattedTo} no tiene un formato válido para Twilio.";
                    }
                    else
                    {
                        userFriendlyMsg = $"Twilio SMS respondió: {errorMsg} (Destino: {formattedTo})";
                    }

                    return new SmsResponseDto
                    {
                        Success = false,
                        Message = userFriendlyMsg,
                        FormattedPhone = formattedTo,
                        Status = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Twilio SMS Exception] Error al comunicar con Twilio SMS para {formattedTo}");
                return new SmsResponseDto
                {
                    Success = false,
                    Message = $"Excepción de red al comunicar con Twilio SMS: {ex.Message}",
                    FormattedPhone = formattedTo
                };
            }
        }

        private static string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            return phone.Replace("whatsapp:", "").Trim();
        }

        public static string FormatSmsRecipient(string phone, string defaultCountryCode)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

            var raw = phone.Replace("whatsapp:", "").Trim();
            var digitsOnly = new string(raw.Where(char.IsDigit).ToArray());

            if (digitsOnly.StartsWith("00"))
            {
                digitsOnly = digitsOnly.Substring(2);
            }

            // Normalización para Argentina
            if (digitsOnly.StartsWith("54"))
            {
                // Dejar como está o ajustar
            }
            else
            {
                if (digitsOnly.StartsWith("0"))
                {
                    digitsOnly = digitsOnly.TrimStart('0');
                }

                string cleanDefault = new string((defaultCountryCode ?? "+54").Where(char.IsDigit).ToArray());
                if (string.IsNullOrEmpty(cleanDefault)) cleanDefault = "54";

                digitsOnly = cleanDefault + digitsOnly;
            }

            // Quitar 15 intermedio si está presente
            // Ejemplo: 549351157439942 o 54351157439942 -> quitar 15
            for (int areaLen = 2; areaLen <= 4; areaLen++)
            {
                // Si empieza con 549 (3 dígitos) o 54 (2 dígitos)
                int prefixLen = digitsOnly.StartsWith("549") ? 3 : (digitsOnly.StartsWith("54") ? 2 : 0);
                int pos15 = prefixLen + areaLen;
                if (digitsOnly.Length >= pos15 + 2 && digitsOnly.Substring(pos15, 2) == "15")
                {
                    var candidate = digitsOnly.Substring(0, pos15) + digitsOnly.Substring(pos15 + 2);
                    // Longitud esperada en Argentina: móvil sin 15 tiene 12 (54 + 10) o 13 (549 + 10) dígitos
                    if (candidate.Length == 12 || candidate.Length == 13)
                    {
                        digitsOnly = candidate;
                        break;
                    }
                }
            }

            return $"+{digitsOnly}";
        }
    }
}
