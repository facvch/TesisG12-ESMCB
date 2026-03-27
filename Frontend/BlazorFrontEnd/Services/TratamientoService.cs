using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class TratamientoService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/tratamientos";

        public TratamientoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TratamientoDto>?> GetByPacienteIdAsync(string pacienteId)
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<List<TratamientoDto>>($"{BaseUrl}/paciente/{pacienteId}");
            }
            catch { return new List<TratamientoDto>(); }
        }

        public async Task<TratamientoDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<TratamientoDto>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateAsync(TratamientoDto modelo)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, modelo);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, TratamientoDto modelo)
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
