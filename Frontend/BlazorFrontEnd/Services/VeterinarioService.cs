using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class VeterinarioService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/veterinario";

        public VeterinarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedList<VeterinarioDto>?> GetAllAsync(bool soloActivos = true)
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<PaginatedList<VeterinarioDto>>($"{BaseUrl}?soloActivos={soloActivos}");
            }
            catch { return null; }
        }

        public async Task<VeterinarioDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<VeterinarioDto>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateAsync(VeterinarioDto v)
        {
            var req = new {
                v.Nombre, v.Apellido, v.Matricula, v.Telefono, v.Email, v.Especialidad
            };
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, req);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, VeterinarioDto v)
        {
            var req = new {
                Id = id, v.Nombre, v.Apellido, v.Telefono, v.Email, v.Especialidad
            };
            var response = await _httpClient.PutAsJsonAsync(BaseUrl, req);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
