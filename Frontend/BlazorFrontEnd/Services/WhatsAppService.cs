using BlazorFrontEnd.Extensions;
using BlazorFrontEnd.Models;
using System.Net.Http.Json;

namespace BlazorFrontEnd.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;

        public WhatsAppService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ═══════════════════════════════
        //  ESTADO DE NOTIFICACIONES
        // ═══════════════════════════════

        public async Task<TwilioStatusDto?> GetStatusAsync()
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<TwilioStatusDto>("api/v1/Sms/status");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsAppService.GetStatusAsync] Error: {ex.Message}");
                return null;
            }
        }

        // ═══════════════════════════════
        //  TWILIO SMS
        // ═══════════════════════════════

        public async Task<SmsResponseDto> SendSmsAsync(string telefono, string mensaje)
        {
            try
            {
                var request = new SendSmsRequest
                {
                    Telefono = telefono,
                    Mensaje = mensaje
                };

                var httpResponse = await _httpClient.PostAsJsonAsync("api/v1/Sms/send", request);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var apiResp = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<SmsResponseDto>>();
                    if (apiResp?.Data != null) return apiResp.Data;

                    var direct = await httpResponse.Content.ReadFromJsonAsync<SmsResponseDto>();
                    return direct ?? new SmsResponseDto { Success = true, Message = "SMS enviado exitosamente." };
                }
                else
                {
                    var errorBody = await httpResponse.Content.ReadAsStringAsync();
                    return new SmsResponseDto
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(errorBody)
                            ? $"Error HTTP {(int)httpResponse.StatusCode}"
                            : errorBody
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsAppService.SendSmsAsync] Error: {ex.Message}");
                return new SmsResponseDto
                {
                    Success = false,
                    Message = $"Error de red: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Envía el SMS de recordatorio para un turno específico (con indicación de 24hs de ayuno si es cirugía o análisis)
        /// </summary>
        public async Task<SmsResponseDto> SendTurnoSmsRecordatorioAsync(string turnoId)
        {
            try
            {
                var httpResponse = await _httpClient.PostAsync($"api/v1/Turno/{turnoId}/enviar-sms-recordatorio", null);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var apiResp = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<SmsResponseDto>>();
                    if (apiResp?.Data != null) return apiResp.Data;

                    var direct = await httpResponse.Content.ReadFromJsonAsync<SmsResponseDto>();
                    return direct ?? new SmsResponseDto { Success = true, Message = "SMS enviado correctamente." };
                }
                else
                {
                    var errorBody = await httpResponse.Content.ReadAsStringAsync();
                    return new SmsResponseDto
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(errorBody)
                            ? $"Error HTTP {(int)httpResponse.StatusCode}"
                            : errorBody
                    };
                }
            }
            catch (Exception ex)
            {
                return new SmsResponseDto { Success = false, Message = $"Excepción al enviar SMS: {ex.Message}" };
            }
        }

        /// <summary>
        /// Envía recordatorios por SMS a todos los turnos del día de mañana
        /// </summary>
        public async Task<HttpResponseMessage> SendTurnosMananaSmsBatchAsync(int? sucursalId = null)
        {
            string url = "api/v1/Recordatorio/turnos/manana/enviar-sms";
            if (sucursalId.HasValue && sucursalId.Value > 0)
                url += $"?sucursalId={sucursalId.Value}";

            return await _httpClient.PostAsync(url, null);
        }

        // ═══════════════════════════════
        //  EMAIL & GOOGLE CALENDAR
        // ═══════════════════════════════

        /// <summary>
        /// Reenvía el email de confirmación de turno con enlace a Google Calendar
        /// </summary>
        public async Task<EmailResponseDto> SendTurnoEmailConfirmacionAsync(string turnoId)
        {
            try
            {
                var httpResponse = await _httpClient.PostAsync($"api/v1/Turno/{turnoId}/enviar-email-confirmacion", null);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var apiResp = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<EmailResponseDto>>();
                    if (apiResp?.Data != null) return apiResp.Data;

                    var direct = await httpResponse.Content.ReadFromJsonAsync<EmailResponseDto>();
                    return direct ?? new EmailResponseDto { Success = true, Message = "Email enviado exitosamente." };
                }
                else
                {
                    var errorBody = await httpResponse.Content.ReadAsStringAsync();
                    return new EmailResponseDto
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(errorBody)
                            ? $"Error HTTP {(int)httpResponse.StatusCode}"
                            : errorBody
                    };
                }
            }
            catch (Exception ex)
            {
                return new EmailResponseDto { Success = false, Message = $"Excepción al enviar email: {ex.Message}" };
            }
        }

        // ═══════════════════════════════
        //  WHATSAPP (COMPATIBILIDAD)
        // ═══════════════════════════════

        public async Task<WhatsAppResponseDto> SendWhatsAppAsync(string telefono, string mensaje)
        {
            try
            {
                var request = new SendWhatsAppRequest
                {
                    Telefono = telefono,
                    Mensaje = mensaje
                };

                var httpResponse = await _httpClient.PostAsJsonAsync("api/v1/WhatsApp/send", request);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var apiResp = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<WhatsAppResponseDto>>();
                    if (apiResp?.Data != null)
                    {
                        return apiResp.Data;
                    }

                    return new WhatsAppResponseDto
                    {
                        Success = true,
                        Message = "Mensaje procesado correctamente por el servidor."
                    };
                }
                else
                {
                    var errorBody = await httpResponse.Content.ReadAsStringAsync();
                    return new WhatsAppResponseDto
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(errorBody)
                            ? $"Error HTTP {(int)httpResponse.StatusCode}"
                            : errorBody
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WhatsAppService.SendWhatsAppAsync] Error: {ex.Message}");
                return new WhatsAppResponseDto
                {
                    Success = false,
                    Message = $"Error de comunicación: {ex.Message}"
                };
            }
        }
    }
}
