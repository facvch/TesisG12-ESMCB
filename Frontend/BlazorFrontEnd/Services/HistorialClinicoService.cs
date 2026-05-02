using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class HistorialClinicoService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/HistorialClinico";

        public HistorialClinicoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedList<HistorialClinicoDto>?> GetAllAsync(int page = 1, int pageSize = 10, string pacienteId = "")
        {
            // The API endpoints for pagination are generic, usually inside PaginacionController
            // However, the HistorialesController might have a get by patient endpoint
            var url = string.IsNullOrEmpty(pacienteId) 
                ? $"api/v1/Paginado/historiales?page={page}&pageSize={pageSize}" // Just in case a general list is needed
                : $"api/v1/HistorialClinico/byPaciente/{pacienteId}"; // Fetch all history items for a specific patient

            try
            {
                if (string.IsNullOrEmpty(pacienteId))
                {
                    return await _httpClient.GetUnwrappedAsync<PaginatedList<HistorialClinicoDto>>(url);
                }
                else
                {
                    // Usually this returns a List, we can wrap it in a mock paginated list if needed,
                    // or just return the list directly. To keep it simple, let's just make a specific method.
                    return null; 
                }
            }
            catch { return null; }
        }

        public async Task<List<HistorialClinicoDto>?> GetByPacienteIdAsync(string pacienteId)
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<List<HistorialClinicoDto>>($"{BaseUrl}/byPaciente/{pacienteId}");
            }
            catch { return new List<HistorialClinicoDto>(); }
        }

        public async Task<HistorialClinicoDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<HistorialClinicoDto>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateAsync(HistorialClinicoDto modelo)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, modelo);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, HistorialClinicoDto modelo)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", modelo);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

