using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class VacunacionService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/registros-vacunacion";

        public VacunacionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RegistroVacunacionDto>?> GetByPacienteIdAsync(string pacienteId)
        {
            try
            {
                // Typically you get everything and filter, or use an endpoint. Assuming an endpoint exists or we use the general one.
                // Depending on the backend mapping it might be at "api/v1/registros-vacunacion/paciente/{id}"
                return await _httpClient.GetUnwrappedAsync<List<RegistroVacunacionDto>>($"{BaseUrl}/paciente/{pacienteId}");
            }
            catch { return new List<RegistroVacunacionDto>(); }
        }

        public async Task<RegistroVacunacionDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<RegistroVacunacionDto>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateAsync(RegistroVacunacionDto modelo)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, modelo);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, RegistroVacunacionDto modelo)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", modelo);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        
        public async Task<List<VacunaDto>?> GetCatalogoVacunasAsync()
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<List<VacunaDto>>("api/v1/vacunas");
            }
            catch { return new List<VacunaDto>(); }
        }
    }
}
