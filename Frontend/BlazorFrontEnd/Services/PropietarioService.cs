using BlazorFrontEnd.Models;
using BlazorFrontEnd.Extensions;
using System.Net.Http.Json;

namespace BlazorFrontEnd.Services
{
    public class PropietarioService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/Propietario";

        public PropietarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedList<PropietarioDto>?> GetAllAsync(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            var url = $"api/v1/Paginado/propietarios?page={page}&pageSize={pageSize}&searchTerm={searchTerm}";
            return await _httpClient.GetUnwrappedAsync<PaginatedList<PropietarioDto>>(url);
        }

        public async Task<List<PropietarioDto>?> GetAllActivosAsync()
        {
            return await _httpClient.GetUnwrappedAsync<List<PropietarioDto>>($"{BaseUrl}");
        }

        public async Task<PropietarioDto?> GetByIdAsync(string id)
        {
            return await _httpClient.GetUnwrappedAsync<PropietarioDto>($"{BaseUrl}/{id}");
        }

        public async Task<bool> CreateAsync(PropietarioDto propietario)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, propietario);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, PropietarioDto propietario)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", propietario);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

