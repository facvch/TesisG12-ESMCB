using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class PacienteService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/Paciente";

        public PacienteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedList<PacienteDto>?> GetAllAsync(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            var url = $"api/v1/Paginado/pacientes?page={page}&pageSize={pageSize}&searchTerm={searchTerm}";
            return await _httpClient.GetUnwrappedAsync<PaginatedList<PacienteDto>>(url);
        }

        public async Task<PacienteDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<PacienteDto>($"{BaseUrl}/{id}");
        }

        public async Task<(bool Success, string ErrorMessage)> CreateAsync(PacienteDto paciente)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, paciente);
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(string id, PacienteDto paciente)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", paciente);
            if (response.IsSuccessStatusCode) return (true, string.Empty);
            var err = await response.Content.ReadAsStringAsync();
            return (false, err);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

