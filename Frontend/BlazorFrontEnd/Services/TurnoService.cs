using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class TurnoService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/Turno";

        public TurnoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedList<TurnoDto>?> GetAllAsync(int page = 1, int pageSize = 10, string searchTerm = "", string? veterinarioId = null)
        {
            var url = $"api/v1/Paginado/turnos?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(searchTerm)) url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (!string.IsNullOrWhiteSpace(veterinarioId)) url += $"&veterinarioId={Uri.EscapeDataString(veterinarioId)}";
            
            return await _httpClient.GetUnwrappedAsync<PaginatedList<TurnoDto>>(url);
        }

        // Endpoint for Calendar View
        public async Task<List<TurnoDto>?> GetRangoAsync(DateTime inicio, DateTime fin, string? searchTerm = null, string? veterinarioId = null)
        {
            var url = $"{BaseUrl}/programados?desde={inicio:s}&hasta={fin:s}";
            if (!string.IsNullOrWhiteSpace(searchTerm)) url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            if (!string.IsNullOrWhiteSpace(veterinarioId)) url += $"&veterinarioId={Uri.EscapeDataString(veterinarioId)}";
            
            return await _httpClient.GetUnwrappedAsync<List<TurnoDto>>(url);
        }

        public async Task<TurnoDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<TurnoDto>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateAsync(TurnoDto turno)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, turno);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, TurnoDto turno)
        {
            // Instead of Full update which might not exist, TurnoController might just have state modifications. 
            // We'll leave it as is, or use the reschedule endpoint if available.
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}/reprogramar", new { NuevaFechaHora = turno.FechaHora, DuracionMinutos = turno.DuracionMinutos });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangeStatusAsync(string id, string newStatus)
        {
            if (newStatus == "Cancelado") {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}/cancelar", "Cancelado desde el sistema");
                return response.IsSuccessStatusCode;
            }
            if (newStatus == "Completado") {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}/completar", "Completado");
                return response.IsSuccessStatusCode;
            }
            return false;
        }
    }
}

