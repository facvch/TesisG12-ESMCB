using BlazorFrontEnd.Models;
using System.Net.Http.Json;
using BlazorFrontEnd.Extensions;

namespace BlazorFrontEnd.Services
{
    public class SucursalService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/v1/sucursal";

        public SucursalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SucursalDto>?> GetAllAsync(bool soloActivos = true)
        {
            try
            {
                var res = await _httpClient.GetUnwrappedAsync<PaginatedList<SucursalDto>>($"{BaseUrl}?soloActivos={soloActivos}");
                return res?.Items ?? new List<SucursalDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SucursalService.GetAllAsync ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<SucursalDto?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetUnwrappedAsync<SucursalDto>($"{BaseUrl}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SucursalService.GetByIdAsync ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateAsync(CreateSucursalRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(SucursalDto request)
        {
            var response = await _httpClient.PutAsJsonAsync(BaseUrl, request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
